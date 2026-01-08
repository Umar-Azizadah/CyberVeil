using System.Collections;
using UnityEngine;

namespace CyberVeil.Systems
{
    /// <summary>
    /// Simple cinematic camera helper that temporarily moves the main camera to focus on a target
    /// And optionally zooms (changes FOV)
    /// Designed for short, scripted cinematic moments
    /// Usage:
    /// - yield return CinematicCamera.Instance.FocusForDuration(target, 2f);
    /// - var handle = CinematicCamera.Instance.StartHoldFocus(target); ... CinematicCamera.Instance.EndHoldFocus();
    /// </summary>
    public class CinematicCamera : MonoBehaviour
    {
        public static CinematicCamera Instance { get; private set; }

        [Header("Defaults")]
        public float defaultTransitionSeconds = 0.35f;
        public float focusDistance = 2.5f;
    public float focusFOV = 40f;
    [Tooltip("Vertical offset applied relative to the target. Positive moves the camera below the target (camera looks up); negative moves it above the target (camera looks down)")]
    public float focusHeight = 0.9f;

        private Camera cam;
        private Transform camTransform;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float originalFOV;

    private Coroutine holdCoroutine;
    private Coroutine transitionCoroutine;
    private bool isHolding = false;
    // true while we've saved the original transform/FOV so we don't overwrite them mid-cinematic
    private bool originalSaved = false;

    /// <summary>
    /// True while a cinematic is active (during transition/hold/restore).
    /// Player code can query this to disable movement while cinematics run.
    /// </summary>
    public bool IsActive => originalSaved || isHolding || holdCoroutine != null || transitionCoroutine != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cam = Camera.main;
            if (cam == null)
            {
                // fallback: create a camera if needed (rare in editor tooling)
                var go = new GameObject("CinematicCamera_MainCam");
                cam = go.AddComponent<Camera>();
            }
            camTransform = cam.transform;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Smoothly move camera to focus on target and hold for duration then restore
        public IEnumerator FocusForDuration(Transform target, float duration, float distance = -1f, float fov = -1f, float transition = -1f)
        {
            if (target == null) yield break;
            if (cam == null) cam = Camera.main;

            if (distance <= 0) distance = focusDistance;
            if (fov <= 0) fov = focusFOV;
            if (transition <= 0) transition = defaultTransitionSeconds;

            // stop any existing hold/transition so we start cleanly
            if (holdCoroutine != null)
            {
                isHolding = false;
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
            }
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            SaveOriginal();

            // Compute target position: keep the camera on the same side of the target as the original camera
            // This avoids moving behind the target when the target's forward is unreliable.
            // keep the camera on the same side relative to the target as the original camera
            Vector3 dir = (originalPosition - target.position).normalized;
            // apply vertical offset relative to the target; positive focusHeight moves the camera below the target (camera looks up)
            Vector3 targetPos = target.position + dir * distance - Vector3.up * focusHeight;
            Quaternion targetRot = Quaternion.LookRotation(target.position - targetPos, Vector3.up);

            // transition
            float t = 0f;
            float startFOV = cam.fieldOfView;
            while (t < transition)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / transition);
                camTransform.position = Vector3.Slerp(originalPosition, targetPos, p);
                camTransform.rotation = Quaternion.Slerp(originalRotation, targetRot, p);
                cam.fieldOfView = Mathf.Lerp(startFOV, fov, p);
                yield return null;
            }

            // hold for duration (unscaled so it works during pauses)
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // restore
            transitionCoroutine = StartCoroutine(Restore(transition));
            yield return transitionCoroutine;
        }

        // Start a hold-style focus, returns coroutine handle, Call EndHoldFocus() to end and restore
        public Coroutine StartHoldFocus(Transform target, float distance = -1f, float fov = -1f, float transition = -1f)
        {
            if (target == null) return null;
            if (distance <= 0) distance = focusDistance;
            if (fov <= 0) fov = focusFOV;
            if (transition <= 0) transition = defaultTransitionSeconds;

            // stop any current transition so we start predictably
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = StartCoroutine(HoldRoutine(target, distance, fov, transition));
            return holdCoroutine;
        }

        public void EndHoldFocus()
        {
            // signal to end; let the coroutine finish its restore so we get a clean snap back
            isHolding = false;
        }

        private IEnumerator HoldRoutine(Transform target, float distance, float fov, float transition)
        {
            if (cam == null) cam = Camera.main;
            SaveOriginal();
            isHolding = true;

            // Compute position similar to FocusForDuration: keep relative to original camera direction
            Vector3 dir = (originalPosition - target.position).normalized;
            Vector3 targetPos = target.position + dir * distance - Vector3.up * focusHeight;
            Quaternion targetRot = Quaternion.LookRotation(target.position - targetPos, Vector3.up);

            float t = 0f;
            float startFOV = cam.fieldOfView;
            while (t < transition)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / transition);
                camTransform.position = Vector3.Slerp(originalPosition, targetPos, p);
                camTransform.rotation = Quaternion.Slerp(originalRotation, targetRot, p);
                cam.fieldOfView = Mathf.Lerp(startFOV, fov, p);
                yield return null;
            }

            // hold until signaled
            while (isHolding)
            {
                // keep the camera looking at the target in case it moves slightly
                camTransform.position = Vector3.Lerp(camTransform.position, targetPos, Time.unscaledDeltaTime * 8f);
                camTransform.rotation = Quaternion.Slerp(camTransform.rotation, targetRot, Time.unscaledDeltaTime * 8f);
                yield return null;
            }

            // restore using transition coroutine so we can track/cancel it if needed
            transitionCoroutine = StartCoroutine(Restore(transition));
            yield return transitionCoroutine;
        }

        private IEnumerator Restore(float transition)
        {
            if (cam == null) cam = Camera.main;
            float t = 0f;
            Vector3 startPos = camTransform.position;
            Quaternion startRot = camTransform.rotation;
            float startFOV = cam.fieldOfView;
            while (t < transition)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / transition);
                camTransform.position = Vector3.Slerp(startPos, originalPosition, p);
                camTransform.rotation = Quaternion.Slerp(startRot, originalRotation, p);
                cam.fieldOfView = Mathf.Lerp(startFOV, originalFOV, p);
                yield return null;
            }
            // ensure exact restore
            camTransform.position = originalPosition;
            camTransform.rotation = originalRotation;
            cam.fieldOfView = originalFOV;
            // clear flags so future cinematics can save the new original transform
            originalSaved = false;
            isHolding = false;
            holdCoroutine = null;
            transitionCoroutine = null;
        }

        private void SaveOriginal()
        {
            if (cam == null) cam = Camera.main;
            camTransform = cam.transform;
            // only save once per cinematic session so we can restore exactly to the pre-cinematic state
            if (!originalSaved)
            {
                originalPosition = camTransform.position;
                originalRotation = camTransform.rotation;
                originalFOV = cam.fieldOfView;
                originalSaved = true;
            }
        }
    }
}

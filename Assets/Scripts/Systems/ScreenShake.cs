using UnityEngine;

namespace CyberVeil.Systems
{
    /// <summary>
    /// Lightweight screen shake that applies a small positional offset to the main camera
    /// Uses a simple "trauma" value so repeated hits stack subtly and decay smoothly
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake instance;

        [Header("Subtle Shake Defaults")]
        [SerializeField, Min(0f)] private float positionalMagnitude = 0.045f;
        [SerializeField, Range(0f, 1f)] private float traumaPerHit = 0.6f;
        [SerializeField, Min(0f)] private float traumaDecayPerSecond = 6f;

        private float trauma;
        private Vector3 lastOffset;

        /// <summary>
        /// Triggers the default subtle shake (intended for player hits on enemies)
        /// Safe to call even if no main camera exists
        /// </summary>
        public static void KickSubtle()
        {
            ScreenShake shake = GetOrCreateOnMainCamera();
            if (shake == null) return;

            shake.AddTrauma(shake.traumaPerHit);
        }

        private static ScreenShake GetOrCreateOnMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                // Fallback if MainCamera tag isn't set
                cam = Object.FindFirstObjectByType<Camera>();
                if (cam == null) return null;
            }

            // If the cached instance is already on the current main camera, reuse it
            if (instance != null && instance.gameObject == cam.gameObject)
                return instance;

            ScreenShake existing = cam.GetComponent<ScreenShake>();
            if (existing == null)
                existing = cam.gameObject.AddComponent<ScreenShake>();

            instance = existing;
            return instance;
        }

        private void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }

        private void LateUpdate()
        {
            // Always remove the previous frame's offset first so we don't drift
            if (lastOffset != Vector3.zero)
            {
                transform.position -= lastOffset;
                lastOffset = Vector3.zero;
            }

            if (trauma <= 0f) return;

            trauma = Mathf.Max(0f, trauma - traumaDecayPerSecond * Time.deltaTime);

            float shakeAmount = positionalMagnitude * trauma;
            if (shakeAmount <= 0f) return;

            Vector2 r = Random.insideUnitCircle;
            Vector3 offset = (transform.right * r.x + transform.up * r.y) * shakeAmount;

            transform.position += offset;
            lastOffset = offset;
        }

        private void OnDisable()
        {
            if (lastOffset != Vector3.zero)
                transform.position -= lastOffset;

            lastOffset = Vector3.zero;
            trauma = 0f;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}

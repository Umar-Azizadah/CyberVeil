using System.Collections;
using CyberVeil.Combat;
using CyberVeil.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CyberVeil.UI
{
    /// <summary>
    /// Adds animated UI feedback to a uGUI health bar when the player's health changes:
    /// - Damage flash on the fill image
    /// - Quick pulse on the container
    /// - Low-health warning glow that pulses while below a threshold
    ///
    /// This script does NOT set the health fill amount by default; it only reacts to health changes.
    /// </summary>
    public class HealthBarFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthComponent health;
        [SerializeField] private Image fillImage;
        [Tooltip("Root RectTransform to pulse on damage. If not set, uses this GameObject.")]
        [SerializeField] private RectTransform container;

        [Header("Damage Flash")]
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.9f, 0.95f, 1f);
        [SerializeField, Min(0f)] private float damageFlashSeconds = 0.1f;

        [Header("HUD Pulse On Damage")]
        [SerializeField, Min(1f)] private float pulseScale = 1.04f;
        [SerializeField, Min(0f)] private float pulseSeconds = 0.18f;

        [Header("Low Health Warning")]
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.30f;
        [Tooltip("Optional glow Image (usually behind the bar) whose alpha will pulse while in low health.")]
        [SerializeField] private Image lowHealthGlow;
        [SerializeField] private Color lowHealthGlowColor = new Color(1f, 0.25f, 0.4f, 1f);
        [SerializeField, Range(0f, 1f)] private float lowHealthGlowMinAlpha = 0.05f;
        [SerializeField, Range(0f, 1f)] private float lowHealthGlowMaxAlpha = 0.18f;
        [SerializeField, Min(0.05f)] private float lowHealthGlowPulseSeconds = 1.6f;

        [Header("Timing")]
        [Tooltip("UI feedback is usually best unscaled so it still plays during hitstop/timeScale changes.")]
        [SerializeField] private bool useUnscaledTime = true;

        private float lastNormalized = 1f;

        private Color originalFillColor;
        private Vector3 originalContainerScale;

        private Coroutine flashCoroutine;
        private Coroutine pulseCoroutine;
        private Coroutine lowHealthCoroutine;

        private bool isLowHealthActive;

        private void Awake()
        {
            if (container == null)
                container = transform as RectTransform;

            if (fillImage != null)
                originalFillColor = fillImage.color;

            if (container != null)
                originalContainerScale = container.localScale;

            if (health == null)
                health = FindPlayerHealthFallback();

            // Initialize glow to off until we decide otherwise
            SetGlowActive(false);
        }

        private void OnEnable()
        {
            if (health == null)
                health = FindPlayerHealthFallback();

            if (health != null)
            {
                lastNormalized = health.Normalized;
                health.OnHealthChanged += HandleHealthChanged;

                // Sync state on enable without triggering damage feedback
                HandleHealthChanged(health);
            }
        }

        private void OnDisable()
        {
            if (health != null)
                health.OnHealthChanged -= HandleHealthChanged;

            StopAndClear(ref flashCoroutine);
            StopAndClear(ref pulseCoroutine);
            StopAndClear(ref lowHealthCoroutine);

            // Restore visuals
            if (fillImage != null)
                fillImage.color = originalFillColor;

            if (container != null)
                container.localScale = originalContainerScale;

            SetGlowActive(false);
        }

        private void HandleHealthChanged(HealthComponent hc)
        {
            if (hc == null) return;

            float normalized = Mathf.Clamp01(hc.Normalized);
            bool tookDamage = normalized < lastNormalized - 0.0001f;

            lastNormalized = normalized;

            UpdateLowHealthState(normalized);

            if (!tookDamage)
                return;

            TriggerDamageFlash();
            TriggerPulse();
        }

        private void TriggerDamageFlash()
        {
            if (fillImage == null) return;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (fillImage != null)
            {
                // Reset to the intended normal color before flashing (prevents drift if spammed)
                fillImage.color = originalFillColor;
                fillImage.color = damageFlashColor;
            }

            float t = 0f;
            float duration = Mathf.Max(0.001f, damageFlashSeconds);
            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);

                if (fillImage != null)
                    fillImage.color = Color.Lerp(damageFlashColor, originalFillColor, p);

                yield return null;
            }

            if (fillImage != null)
                fillImage.color = originalFillColor;

            flashCoroutine = null;
        }

        private void TriggerPulse()
        {
            if (container == null) return;

            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);

            pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            if (container != null)
                container.localScale = originalContainerScale;

            Vector3 start = originalContainerScale;
            Vector3 peak = originalContainerScale * pulseScale;

            float duration = Mathf.Max(0.001f, pulseSeconds);
            float half = duration * 0.5f;

            // Up
            float t = 0f;
            while (t < half)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float p = Mathf.Clamp01(t / half);
                float eased = 1f - Mathf.Pow(1f - p, 3f); // ease-out

                if (container != null)
                    container.localScale = Vector3.LerpUnclamped(start, peak, eased);

                yield return null;
            }

            // Down
            t = 0f;
            while (t < half)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float p = Mathf.Clamp01(t / half);
                float eased = Mathf.Pow(p, 3f); // ease-in

                if (container != null)
                    container.localScale = Vector3.LerpUnclamped(peak, start, eased);

                yield return null;
            }

            if (container != null)
                container.localScale = start;

            pulseCoroutine = null;
        }

        private void UpdateLowHealthState(float normalized)
        {
            bool shouldBeLow = normalized <= lowHealthThreshold;
            if (shouldBeLow == isLowHealthActive)
                return;

            isLowHealthActive = shouldBeLow;

            if (isLowHealthActive)
            {
                if (lowHealthGlow != null)
                {
                    SetGlowActive(true);
                    if (lowHealthCoroutine != null)
                        StopCoroutine(lowHealthCoroutine);

                    lowHealthCoroutine = StartCoroutine(LowHealthGlowRoutine());
                }
            }
            else
            {
                StopAndClear(ref lowHealthCoroutine);
                SetGlowActive(false);
            }
        }

        private IEnumerator LowHealthGlowRoutine()
        {
            if (lowHealthGlow == null)
            {
                lowHealthCoroutine = null;
                yield break;
            }

            float seconds = Mathf.Max(0.05f, lowHealthGlowPulseSeconds);
            float omega = (Mathf.PI * 2f) / seconds;
            float t = 0f;

            while (isLowHealthActive && lowHealthGlow != null)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt;
                float s = (Mathf.Sin(t * omega) + 1f) * 0.5f;

                float a = Mathf.Lerp(lowHealthGlowMinAlpha, lowHealthGlowMaxAlpha, s);
                Color c = lowHealthGlowColor;
                c.a = a;
                lowHealthGlow.color = c;

                yield return null;
            }

            lowHealthCoroutine = null;
        }

        private void SetGlowActive(bool active)
        {
            if (lowHealthGlow == null) return;

            if (!active)
            {
                // Make sure there's no residual glow
                Color c = lowHealthGlow.color;
                c.a = 0f;
                lowHealthGlow.color = c;
                lowHealthGlow.enabled = false;
            }
            else
            {
                lowHealthGlow.enabled = true;
            }
        }

        private void StopAndClear(ref Coroutine routine)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = null;
        }

        private static HealthComponent FindPlayerHealthFallback()
        {
            // Fallback for convenience if the reference isn't wired in the inspector.
            // Picks the first HealthComponent marked as Player.
            HealthComponent[] all = Object.FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].faction == Faction.Player)
                    return all[i];
            }
            return null;
        }
    }
}

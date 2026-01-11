using UnityEngine;
using CyberVeil.Combat;

namespace CyberVeil.UI
{
    /// <summary>
    /// UI controller for displaying a health bar
    /// Listens to HealthComponent and resizes the bar smoothly based on the current health fraction
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private HealthComponent health; // Referencing, data source which raises an event when health changes
        [SerializeField] private RectTransform bar; // UI slice that acttually shrinks or grows
        [Range(0f, 30f)] public float smoothSpeed = 12f;

        [Header("Visual Range Calibration")]
        [Tooltip("Health bar width fraction when health is 0% (empty). If your bar looks empty at 0.45, set this to 0.45.")]
        [Range(0f, 1f)] public float visualEmpty = 0.45f;
        [Tooltip("Health bar width fraction when health is 100% (full).")]
        [Range(0f, 1f)] public float visualFull = 1f;

       [SerializeField] private float currentFill = 1f, targetFill = 1f; // What the bar shows right now and what the bar is supposed to be at [0..1] percentage
        private float initialWidth;

        private void OnEnable()
        {
            // Subscribes the method to the event so when HealthComponent fires OnHealthChanged, HandleHealthChanges runs
            if (health != null) health.OnHealthChanged += HandleHealthChanged;
        }
        private void OnDisable()
        {
            // Unsubscribes, prevents leaks
            if (health != null) health.OnHealthChanged -= HandleHealthChanged;
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases(); // Makes sure layout is baked before reading rect sizes
            initialWidth = bar.rect.width;

            // Set both the display and the target to the health fraction if health exists, otherwise assume full health (1.0)
            currentFill = targetFill = health != null ? health.Normalized : 1f; // Safe fallback so healthbar doesnt break
            ApplyFill(currentFill);
        }

        private void Update()
        {
            if (Mathf.Abs(currentFill - targetFill) > 0.0005f) // Only bother animating if the bar is noticeably different from the target
            {
                // Slides the bar smoothly towards the real health at a rate controlled by smoothSpeed
                currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);

                // If distance between the two is close enough, set them to equal so stops animating cleanly
                if (Mathf.Abs(currentFill - targetFill) < 0.001f)
                    currentFill = targetFill;

                ApplyFill(currentFill); // Apllies the new currentFill to the actual UI bar
            }
        }

        /// <summary>
        /// Event handler for HealthComponent
        /// When health changes, it updates the target value, so the bar knows where it eventually should be
        /// </summary>
        private void HandleHealthChanged(HealthComponent hc)
        {
            targetFill = hc.Normalized;
        }

        private void ApplyFill(float fill01)
        {
            fill01 = Mathf.Clamp01(fill01); // Clamps the input so it never goes below 0 or above 1

            // Map health (0→1) to visual range (visualEmpty→visualFull)
            float visualFill = Mathf.Lerp(visualEmpty, visualFull, fill01);

            // Resizes the bar to the correct width (in whole pixels), based on visual percent × initial width
            float widthPx = Mathf.Round(visualFill * initialWidth);
            bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthPx);
        }
    }
}

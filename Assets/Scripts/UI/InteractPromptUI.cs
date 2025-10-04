using UnityEngine;
using TMPro;

namespace CyberVeil.UI
{
    /// <summary>
    /// Simple prompt widget that shows a short action hint like "[E] Talk"
    /// Uses CanvasGroup alpha when available, otherwise toggles GameObject active state
    /// </summary>
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text label;

        private void Awake() // Starts hidden
        {
            if (group != null) group.alpha = 0f;
        }

        public void Show(string text) // Shows the prompt with the privided text
        {
            if (label != null) label.text = text;
            if (group != null) group.alpha = 1f;
            else gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (group != null) group.alpha = 0f;
            else gameObject.SetActive(false);
        }
    }
}

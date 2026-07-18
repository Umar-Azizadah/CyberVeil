using UnityEngine;
using TMPro;
using System.Collections;

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
        [SerializeField] private bool useTypewriter = true;
        [SerializeField] private float charsPerSecond = 65f;

        private Coroutine typewriteRoutine;

        private void Awake() // Starts hidden
        {
            if (group != null) group.alpha = 0f;
        }

        public void Show(string text) // Shows the prompt with the provided text
        {
            if (typewriteRoutine != null)
                StopCoroutine(typewriteRoutine);

            if (group != null) group.alpha = 1f;
            else gameObject.SetActive(true);

            if (useTypewriter && label != null)
                typewriteRoutine = StartCoroutine(TextTypewriterUtility.TypewriteText(label, text, charsPerSecond));
            else if (label != null)
                label.text = text;
        }

        public void Hide()
        {
            if (typewriteRoutine != null)
                StopCoroutine(typewriteRoutine);

            if (label != null)
                TextTypewriterUtility.ResetText(label);

            if (group != null) group.alpha = 0f;
            else gameObject.SetActive(false);
        }
    }
}

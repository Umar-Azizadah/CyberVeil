// CyberVeil.UI
using System.Collections;
using TMPro;
using UnityEngine;

namespace CyberVeil.UI
{
    /// <summary>
    /// Displays a single line of dialogue and can be shown/hidden
    /// Driven by an interactable's coroutine to page through lines
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text body;
        [Header("Animation")]
        [SerializeField] private bool useTypewriter = true;
        [SerializeField] private float charsPerSecond = 45f;
        [SerializeField] private bool usePopScale = true;
        [SerializeField] private float popScale = 1.05f;
        [SerializeField] private float popDuration = 0.12f;

        private Coroutine showRoutine;
        private Vector3 bodyBaseScale = Vector3.one;

        private void Awake()
        {
            if (body != null)
                bodyBaseScale = body.rectTransform.localScale;
        }

        public void ShowLine(string line) // Shows the dialogue UI and sets the given line of text
        {
            if (showRoutine != null)
                StopCoroutine(showRoutine);

            if (group) group.alpha = 1f;
            gameObject.SetActive(true);

            if (body == null)
                return;

            showRoutine = StartCoroutine(ShowLineRoutine(line));
        }

        public void Hide()
        {
            if (showRoutine != null)
                StopCoroutine(showRoutine);

            if (body != null)
            {
                body.maxVisibleCharacters = int.MaxValue;
                body.rectTransform.localScale = bodyBaseScale;
            }

            if (group) group.alpha = 0f;
            gameObject.SetActive(false);
        }

        private IEnumerator ShowLineRoutine(string line)
        {
            if (usePopScale)
                yield return StartCoroutine(PopScale());

            if (!useTypewriter)
            {
                body.text = line;
                body.maxVisibleCharacters = int.MaxValue;
                yield break;
            }

            yield return TextTypewriterUtility.TypewriteText(body, line, charsPerSecond);
        }

        private IEnumerator PopScale()
        {
            if (body == null)
                yield break;

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, popDuration);
            Vector3 targetScale = bodyBaseScale * popScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                body.rectTransform.localScale = Vector3.Lerp(bodyBaseScale, targetScale, t);
                yield return null;
            }

            body.rectTransform.localScale = bodyBaseScale;
        }
    }
}

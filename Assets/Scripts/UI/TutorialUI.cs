using UnityEngine;
using TMPro;
using System.Collections;
using System;

namespace CyberVeil.UI
{
    /// <summary>
    /// Displays tutorial text on screen for a set duration
    /// Each line appears for a configurable amount of time before moving to the next
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        public static event Action OnTutorialComplete;

        [SerializeField] private TMP_Text tutorialText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float displayDuration = 2f; // How long each line shows
        [SerializeField] private float fadeDuration = 0.3f;  // Fade in/out duration
        [SerializeField] private bool useTypewriter = true;
        [SerializeField] private float charsPerSecond = 65f;

        private Coroutine tutorialCoroutine;

        private void Awake()
        {
            if (tutorialText == null)
                tutorialText = GetComponentInChildren<TMP_Text>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Shows a sequence of tutorial lines, each for the specified duration
        /// </summary>
        public void ShowTutorial(string[] tutorialLines)
        {
            if (tutorialCoroutine != null)
                StopCoroutine(tutorialCoroutine);

            tutorialCoroutine = StartCoroutine(TutorialCoroutine(tutorialLines));
        }

        private IEnumerator TutorialCoroutine(string[] lines)
        {
            yield return new WaitForSeconds(2f);
            gameObject.SetActive(true);

            foreach (string line in lines)
            {
                // Fade in
                yield return StartCoroutine(FadeInCoroutine(line));

                // Display for duration
                yield return new WaitForSeconds(displayDuration);

                // Fade out
                yield return StartCoroutine(FadeOutCoroutine());
            }

            gameObject.SetActive(false);
            OnTutorialComplete?.Invoke();
        }

        private IEnumerator FadeInCoroutine(string text)
        {
            if (canvasGroup == null) yield break;

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;

            // Apply typewriter animation after fade in
            if (useTypewriter && tutorialText != null)
                yield return TextTypewriterUtility.TypewriteText(tutorialText, text, charsPerSecond);
            else if (tutorialText != null)
                tutorialText.text = text;
        }

        private IEnumerator FadeOutCoroutine()
        {
            if (canvasGroup == null) yield break;

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        public void Stop()
        {
            if (tutorialCoroutine != null)
                StopCoroutine(tutorialCoroutine);

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
        }
    }
}

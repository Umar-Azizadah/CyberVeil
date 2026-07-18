using System.Collections;
using TMPro;
using UnityEngine;

namespace CyberVeil.UI
{
    /// <summary>
    /// Utility for creating typewriter animation effects on TMP_Text
    /// Provides consistent typewriter animation across all UI text elements
    /// </summary>
    public static class TextTypewriterUtility
    {
        /// <summary>
        /// Animates text with a typewriter effect by revealing characters one at a time
        /// </summary>
        /// <param name="textComponent">The TMP_Text to animate</param>
        /// <param name="text">The text to display</param>
        /// <param name="charsPerSecond">How many characters to reveal per second</param>
        /// <returns>Coroutine that can be started with StartCoroutine()</returns>
        public static IEnumerator TypewriteText(TMP_Text textComponent, string text, float charsPerSecond = 45f)
        {
            if (textComponent == null)
                yield break;

            textComponent.text = text;
            textComponent.ForceMeshUpdate();

            int total = textComponent.textInfo.characterCount;
            textComponent.maxVisibleCharacters = 0;

            if (total <= 0)
                yield break;

            float secondsPerChar = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;
            for (int i = 0; i <= total; i++)
            {
                textComponent.maxVisibleCharacters = i;
                if (secondsPerChar > 0f)
                    yield return new WaitForSeconds(secondsPerChar);
                else
                    yield return null;
            }
        }

        /// <summary>
        /// Resets a TMP_Text to show all characters
        /// </summary>
        public static void ResetText(TMP_Text textComponent)
        {
            if (textComponent != null)
                textComponent.maxVisibleCharacters = int.MaxValue;
        }
    }
}

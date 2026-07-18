using System.Collections;
using TMPro;
using UnityEngine;
using CyberVeil.Systems;

namespace CyberVeil.UI
{
    /// <summary>
    /// Shows a short on-screen message when a trial curse is applied.
    /// </summary>
    public class CurseDisplayUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text messageText;

        [Header("Timing")]
        [SerializeField] private float fadeInSeconds = 0.25f;
        [SerializeField] private float displaySeconds = 2.5f;
        [SerializeField] private float fadeOutSeconds = 0.4f;

        [Header("Typewriter")]
        [SerializeField] private bool useTypewriter = true;
        [SerializeField] private float charsPerSecond = 65f;

        private Coroutine showRoutine;

        private void Awake()
        {
            if (group != null)
                group.alpha = 0f;
        }

        private void OnEnable()
        {
            TrialCurseModifier.OnCurseApplied += HandleCurseApplied;
            TrialCurseModifier.OnCurseCleared += HandleCurseCleared;
        }

        private void OnDisable()
        {
            TrialCurseModifier.OnCurseApplied -= HandleCurseApplied;
            TrialCurseModifier.OnCurseCleared -= HandleCurseCleared;
        }

        private void HandleCurseApplied(TrialCurseModifier.CurseType curse, int bonusSlots)
        {
            (string title, string body) = GetCurseText(curse);

            string msg = string.IsNullOrEmpty(body) ? title : $"{title}\n{body}";
            if (bonusSlots > 0)
                msg = $"{msg}\nBONUS UPGRADE +{bonusSlots}";

            if (showRoutine != null)
                StopCoroutine(showRoutine);
            showRoutine = StartCoroutine(ShowRoutine(msg));
        }

        private void HandleCurseCleared()
        {
            if (showRoutine != null)
                StopCoroutine(showRoutine);
            showRoutine = StartCoroutine(FadeTo(0f, fadeOutSeconds));
        }

        private IEnumerator ShowRoutine(string message)
        {
            yield return FadeTo(1f, fadeInSeconds);
            
            // Apply typewriter animation
            if (useTypewriter && messageText != null)
                yield return TextTypewriterUtility.TypewriteText(messageText, message, charsPerSecond);
            else if (messageText != null)
                messageText.text = message;

            yield return new WaitForSeconds(displaySeconds);
            yield return FadeTo(0f, fadeOutSeconds);
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            if (group == null)
                yield break;

            float start = group.alpha;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                group.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            group.alpha = target;
        }

        private static (string, string) GetCurseText(TrialCurseModifier.CurseType curse)
        {
            switch (curse)
            {
                case TrialCurseModifier.CurseType.DoubleEnemySpeed:
                    return ("CURSE: DOUBLE SPEED", "Enemies move twice as fast.");
                case TrialCurseModifier.CurseType.PlayerChipDamage:
                    return ("CURSE: CHIP DAMAGE", "Player takes damage over time.");
                case TrialCurseModifier.CurseType.ReducedAttackLimit:
                    return ("CURSE: BLADE LIMIT", "Only 3 swings before lockout.");
                default:
                    return (string.Empty, string.Empty);
            }
        }
    }
}

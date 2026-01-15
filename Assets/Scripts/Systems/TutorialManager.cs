using UnityEngine;
using CyberVeil.UI;

namespace CyberVeil.Systems
{
    /// <summary>
    /// Manages the game tutorial sequence
    /// Shows control instructions at the start of gameplay
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private TutorialUI tutorialUI;
        [SerializeField] private bool showTutorialOnStart = true;

        private static string[] controlsTutorial = new string[]
        {
            "WASD — Move",
            "Mouse — Look",
            "Left Click — Slash",
            "Space — Teleport (Blink)",
            "Chain up to 4 slashes, blink to reset your blade",
        };

        private void Start()
        {
            if (showTutorialOnStart && tutorialUI != null)
            {
                ShowControlsTutorial();
            }
        }

        public void ShowControlsTutorial()
        {
            if (tutorialUI != null)
            {
                tutorialUI.ShowTutorial(controlsTutorial);
            }
        }

        public void ShowCustomTutorial(string[] customLines)
        {
            if (tutorialUI != null)
            {
                tutorialUI.ShowTutorial(customLines);
            }
        }

        public void StopTutorial()
        {
            if (tutorialUI != null)
            {
                tutorialUI.Stop();
            }
        }
    }
}

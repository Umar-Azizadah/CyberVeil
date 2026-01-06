using CyberVeil.Core;
using CyberVeil.UI;
using System.Collections;
using UnityEngine;

namespace CyberVeil.World
{
    [DisallowMultipleComponent]
    /// <summary>
    /// Interactable NPC that runs a timed multi-line conversation
    /// Hides the interact prompt during dialogue and restores it when finished
    /// </summary>
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        [Header("UI")]
        [SerializeField] private string npcName = "Veymar";
        [SerializeField] private string prompt = "Talk";
        public string Prompt => prompt;

        [Header("Dialogue")]
        [TextArea]
        [SerializeField]
        private string[] lines = // Lines the npc will say in order
        {
            "The Veil fractures, crimson blight spills throughout.",
            "Corruption devours the very veins of neon that once lit our world.",
            "The hordes that comes are forged, reworked in the corruptions guise",
            "Cleanse the crystal, and when the slaughter wanes, the portal shall shape its remnants into power."
        };
        [SerializeField] private DialogueUI dialogue;
        [SerializeField] private float autoAdvanceSeconds = 4f;

        private Coroutine convo;

        [System.Obsolete]
        /// <summary>
        /// Starts the conversation: hides the prompt, then pages through the configured lines
        /// </summary>
        public void Interact(IInteractor interactor) // Called by the PlayerInteractor when the player presses E on this object
        {
            if (convo != null)
                StopCoroutine(convo);

            var promptUI = FindObjectOfType<InteractPromptUI>(true);
            if (promptUI) promptUI.gameObject.SetActive(false);

            convo = StartCoroutine(RunConversation(interactor));
        }

        [System.Obsolete]
        /// <summary>
        /// Coroutine that displays each line for "autoAdvanceSeconds" seconds
        /// </summary>
        private IEnumerator RunConversation(IInteractor interactor)
        {
            for (int i = 0; i < lines.Length; i++) // Iterate in order through all dialogue lines
            {
                dialogue?.ShowLine(lines[i]);
                yield return new WaitForSeconds(autoAdvanceSeconds);
            }

            dialogue?.Hide();
            // Re-find and re-enable the prompt UI
            var promptUI = FindObjectOfType<InteractPromptUI>(true);
            if (promptUI) promptUI.gameObject.SetActive(true);
            
            convo = null;
        }

        public void OnFocus(IInteractor interactor) { /* add future visuals? */ }
        public void OnDefocus(IInteractor interactor) { /* add future visuals?*/ }
    }
}

using CyberVeil.Core;
using CyberVeil.UI;
using System.Collections;
using UnityEngine;
using CyberVeil.Systems;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

namespace CyberVeil.World
{
    [DisallowMultipleComponent]
    /// <summary>
    /// Interactable upgrade portal that opens the UpgradeMenu and waits until it closes,
    /// hiding the interact prompt during the interaction
    /// </summary>
    public class UpgradePortalInteractable : MonoBehaviour, IInteractable
    {
        [Header("UI")]
        [SerializeField] private string portalName = "Upgrade Portal";
        [SerializeField] private string prompt = "Upgrade";
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private string incompleteWavesMessage = "Clear all corrupted";
        public string Prompt => prompt;

        private NameTag nameTag;

        private Coroutine flow;

        [System.Obsolete]
        
        /// <summary>
        /// Opens the upgrade menu and hides the interact prompt while the menu is active
        /// </summary>
        public void Interact(IInteractor interactor) // Called by the PlayerInteractor when the player presses E on this object
        {
            if (flow != null) // If an interaction coroutine is already running stop it to prevent overlapping flows
                StopCoroutine(flow);

            var promptUI = FindObjectOfType<InteractPromptUI>(true);
            if (promptUI) promptUI.gameObject.SetActive(false);

            flow = StartCoroutine(RunInteraction(interactor));
        }

        private void Awake()
        {
            // Cache optional name tag if present on the prefab and initialize it
            nameTag = GetComponentInChildren<NameTag>(true);
            if (nameTag != null)
            {
                nameTag.Show(false);
            }
        }

        [System.Obsolete]
        /// <summary>
        /// Coroutine that shows the upgrade menu and waits until it closes
        /// </summary>
        private IEnumerator RunInteraction(IInteractor interactor)
        {
            // Check if all waves are complete
            WaveManager waveManager = FindObjectOfType<WaveManager>();
            if (waveManager != null && !waveManager.AreAllWavesComplete())
            {
                // Show "Clear all corrupted" message
                if (dialogueUI != null)
                {
                    dialogueUI.ShowLine(incompleteWavesMessage);
                    yield return new WaitForSeconds(2.5f);
                    dialogueUI.Hide();
                }

                var promptUI = FindObjectOfType<InteractPromptUI>(true);
                if (promptUI) promptUI.gameObject.SetActive(true);

                yield break;
            }

            // Start a cinematic hold on the portal and keep it active while the upgrade menu is open
            bool holdStarted = false;
            if (CinematicCamera.Instance != null)
            {
                CinematicCamera.Instance.StartHoldFocus(transform);
                holdStarted = true;
            }

            try
            {
                // small delay so the camera has time to move in before the menu appears
                yield return new WaitForSecondsRealtime(1f);

                if (UpgradeMenu.Instance != null)
                {
                    yield return UpgradeMenu.Instance.ShowAndWait();
                }
            }
            finally
            {
                if (holdStarted && CinematicCamera.Instance != null)
                {
                    CinematicCamera.Instance.EndHoldFocus();
                }
            }

            var promptUI2 = FindObjectOfType<InteractPromptUI>(true);
            if (promptUI2) promptUI2.gameObject.SetActive(true);

            flow = null;
        }

        public void OnFocus(IInteractor interactor) {
            if (nameTag != null) nameTag.Show(true);
        }
        public void OnDefocus(IInteractor interactor) {
            if (nameTag != null) nameTag.Show(false); 
        }
    }
}

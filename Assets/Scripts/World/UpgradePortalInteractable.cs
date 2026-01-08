using CyberVeil.Core;
using CyberVeil.UI;
using System.Collections;
using UnityEngine;
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
        public string Prompt => prompt;

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

        [System.Obsolete]
        /// <summary>
        /// Coroutine that shows the upgrade menu and waits until it closes
        /// </summary>
        private IEnumerator RunInteraction(IInteractor interactor)
        {
            // Start a cinematic hold on the portal and keep it active while the upgrade menu is open
            bool holdStarted = false;
            if (CyberVeil.Systems.CinematicCamera.Instance != null)
            {
                CyberVeil.Systems.CinematicCamera.Instance.StartHoldFocus(transform);
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
                if (holdStarted && CyberVeil.Systems.CinematicCamera.Instance != null)
                {
                    CyberVeil.Systems.CinematicCamera.Instance.EndHoldFocus();
                }
            }

            var promptUI = FindObjectOfType<InteractPromptUI>(true);
            if (promptUI) promptUI.gameObject.SetActive(true);

            flow = null;
        }

        public void OnFocus(IInteractor interactor) { /* add future visuals? */ }
        public void OnDefocus(IInteractor interactor) { /* add future visuals? */ }
    }
}

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
        public void Interact(IInteractor interactor)
        {
            if (flow != null)
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
            if (UpgradeMenu.Instance != null)
            {
                yield return UpgradeMenu.Instance.ShowAndWait();
            }

            var promptUI = FindObjectOfType<InteractPromptUI>(true);
            if (promptUI) promptUI.gameObject.SetActive(true);

            flow = null;
        }

        public void OnFocus(IInteractor interactor) { /* add future visuals? */ }
        public void OnDefocus(IInteractor interactor) { /* add future visuals? */ }
    }
}

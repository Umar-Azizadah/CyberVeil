using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CyberVeil.UI
{
    /// <summary>
    /// Controls the Upgrade Menu UI
    /// Handles showing/hiding the menu, pausing the game while it’s open,
    /// and waiting for the player to select an upgrade option.
    /// </summary>
    public class UpgradeMenu : MonoBehaviour
    {
        public static UpgradeMenu Instance; // Reference to the menu

        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button[] cardButtons; // Array holding references to the card buttons

        public bool IsOpen { get; private set; } // Tells if menu is currently open 
        public int? PickedIndex { get; private set; } // Which card index was chosen

        void Awake()
        {
            Instance = this;
            WireButtons();
            HideImmediate();
        }

        /// <summary>
        /// Connects all upgrade card buttons to the OnPick() callback
        /// Ensures that each button calls with the correct index when clicked
        /// </summary>
        private void WireButtons()
        {
            for (int i = 0; i < cardButtons.Length; i++) // Looping through buttons
            {
                int idx = i; // Stores i to capture the correct index
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => OnPick(idx));
            }
        }

        /// <summary>
        /// Called when a player clicks an upgrade button
        /// Records which button was picked by storing its index
        /// </summary>
        private void OnPick(int index)
        {
            PickedIndex = index; // Save which card was chosen
        }

        /// <summary>
        /// Makes the upgrade menu visible, interactive,
        /// pauses the game, and unlocks the mouse cursor
        /// </summary>
        private void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            IsOpen = true;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Starts menu off hidden when game starts
        /// </summary>
        private void HideImmediate()
        {
            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
            IsOpen = false;
        }

        /// <summary>
        /// Hide to close menu and resume normal gameplay
        /// </summary>
        private void Hide()
        {
            HideImmediate();
            Time.timeScale = 1f;                 // resume if you paused
            Cursor.visible = false;              // or keep true if your game uses mouse
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Opens the menu and waits until the player clicks a card
        /// Returns when a card is picked; use PickedIndex to know which one
        /// </summary>
        public IEnumerator ShowAndWait()
        {
            PickedIndex = null; //Reset picked index
            Show();

            // Wait until a card is clicked
            while (PickedIndex == null)
                yield return null;

            // I Can apply upgrade here by using an upgrade manager
            // UpgradeManager.Instance.ApplyUpgrade(PickedIndex.Value);

            Hide(); // Hides after choice is made 
        }
    }
}

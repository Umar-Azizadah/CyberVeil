using System.Collections;
using CyberVeil.Combat;
using CyberVeil.Core;
using CyberVeil.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberVeil.UI
{
    /// <summary>
    /// Handles player death dialog and restart functionality
    /// Shows "Corrupted, press Z to restart" when player dies
    /// Reloads the current scene when Z is pressed
    /// </summary>
    public class DeathDialogHandler : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogUI;
        private bool playerDead = false;
        private bool restarting = false;

        private void Start()
        {
            // Find player and subscribe to death
            HealthComponent playerHealth = FindPlayerHealth();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += OnPlayerHealthChanged;
            }
        }

        private void OnDestroy()
        {
            HealthComponent playerHealth = FindPlayerHealth();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
            }
        }

        private void Update()
        {
            if (playerDead && !restarting && Input.GetKeyDown(KeyCode.Z))
            {
                restarting = true;
                StartCoroutine(RestartLevel());
            }
        }

        private void OnPlayerHealthChanged(HealthComponent health)
        {
            // Check if health is zero and player
            if (health.faction == Faction.Player && health.Normalized <= 0f && !playerDead)
            {
                playerDead = true;
                ShowDeathDialog();
            }
        }

        private void ShowDeathDialog()
        {
            if (dialogUI != null)
            {
                Time.timeScale = 0f; // Pause the game
                dialogUI.ShowLine("Corrupted, press Z to restart");
            }
        }

        private IEnumerator RestartLevel()
        {
            Time.timeScale = 1f; // Resume game time
            
            ScreenFadeManager fadeManager = ScreenFadeManager.Instance;
            if (fadeManager != null)
            {
                fadeManager.RequestFadeFromBlackOnNextScene();

                // Fade to black and load scene
                fadeManager.FadeToBlack(() =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
            else
            {
                // Fallback if fade manager not available
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            yield break;
        }

        private static HealthComponent FindPlayerHealth()
        {
            HealthComponent[] all = FindObjectsOfType<HealthComponent>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].faction == Faction.Player)
                    return all[i];
            }
            return null;
        }
    }
}

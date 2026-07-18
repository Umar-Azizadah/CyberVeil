using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace CyberVeil.Systems
{
    /// <summary>
    /// Manages screen fading effects using a canvas with black overlay
    /// Fades in (to black) and out (from black) for scene transitions
    /// Dynamically finds the BlackFade canvas in each scene
    /// </summary>
    public class ScreenFadeManager : MonoBehaviour
    {
        public static ScreenFadeManager Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeInDuration = 1.0f;   // Time to fade to black
        [SerializeField] private float fadeOutDuration = 1.5f;  // Time to fade from black

        private Coroutine fadeCoroutine;
        private bool isFirstLoad = true;
        private bool fadeFromBlackOnNextScene = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Only DontDestroyOnLoad if this is a root object
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Debug.LogWarning("ScreenFadeManager must be on a root GameObject for DontDestroyOnLoad to work!");
                }
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Try to auto-find the canvas group if not assigned
            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = GetComponent<CanvasGroup>();
            }

            // Keep GameObject active so coroutines can run, but make canvas invisible
            gameObject.SetActive(true);
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }

            // Listen for scene loads to update canvas reference
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        /// <summary>
        /// Called when a new scene loads - finds the BlackFade canvas and sets it to fully black
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Find the BlackFade canvas in the new scene
            UpdateCanvasReference();

            if (fadeFromBlackOnNextScene)
            {
                fadeFromBlackOnNextScene = false;
                if (fadeCanvasGroup != null)
                {
                    fadeCanvasGroup.alpha = 1f;
                    FadeFromBlack();
                }
                return;
            }

            // On first load, fade out from black for startup
            if (isFirstLoad)
            {
                isFirstLoad = false;
                if (fadeCanvasGroup != null)
                {
                    fadeCanvasGroup.alpha = 1f;
                    FadeFromBlack();
                }
            }
            else
            {
                // For subsequent scene loads, make sure we're immediately fully black
                if (fadeCanvasGroup != null)
                {
                    fadeCanvasGroup.alpha = 1f;
                }
            }
        }

        /// <summary>
        /// Looks for the BlackFade canvas in the current scene
        /// </summary>
        private void UpdateCanvasReference()
        {
            // Try to find BlackFade canvas in the scene by name
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.name.Contains("BlackFade") || canvas.gameObject.name.Contains("blackfade"))
                {
                    fadeCanvasGroup = canvas.GetComponent<CanvasGroup>();
                    if (fadeCanvasGroup != null)
                    {
                        Debug.Log("ScreenFadeManager: Found BlackFade canvas in new scene");
                        return;
                    }
                }
            }

            // If still not found, look for a CanvasGroup specifically on objects named BlackFade
            Transform[] allTransforms = FindObjectsOfType<Transform>(true);
            foreach (Transform transform in allTransforms)
            {
                if (transform.gameObject.name.Contains("BlackFade") || transform.gameObject.name.Contains("blackfade"))
                {
                    fadeCanvasGroup = transform.GetComponent<CanvasGroup>();
                    if (fadeCanvasGroup != null)
                    {
                        Debug.Log("ScreenFadeManager: Found BlackFade CanvasGroup in new scene");
                        return;
                    }
                }
            }

            Debug.LogWarning("ScreenFadeManager: Could not find BlackFade canvas in scene. Make sure it exists and is named 'BlackFade'");
        }

        /// <summary>
        /// Fades the screen to black, then optionally triggers a callback when fade completes
        /// Typically called before loading a new scene
        /// </summary>
        public void FadeToBlack(System.Action onComplete = null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeToBlackCoroutine(onComplete));
        }

        /// <summary>
        /// Fades the screen from black back to normal visibility
        /// Typically called after the new scene loads
        /// </summary>
        public void FadeFromBlack()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeFromBlackCoroutine());
        }

        /// <summary>
        /// Ensures the next scene load fades from black on arrival.
        /// </summary>
        public void RequestFadeFromBlackOnNextScene()
        {
            fadeFromBlackOnNextScene = true;
        }

        private IEnumerator FadeToBlackCoroutine(System.Action onComplete = null)
        {
            if (fadeCanvasGroup == null) yield break;

            // Make sure GameObject is active for coroutine execution
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;

            // Fade in to black
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;

            // Callback when fully black
            onComplete?.Invoke();
        }

        private IEnumerator FadeFromBlackCoroutine()
        {
            if (fadeCanvasGroup == null) yield break;

            // Make sure GameObject is active and fully black
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;

            // Fade out from black
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
                yield return null;
            }

            fadeCanvasGroup.alpha = 0f;

            // Keep GameObject active, just invisible
        }

        /// <summary>
        /// Instantly hides the fade canvas
        /// </summary>
        public void HideImmediately()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                // Keep GameObject active for future fades
            }
        }
    }
}

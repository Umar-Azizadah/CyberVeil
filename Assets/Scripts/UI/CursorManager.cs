using UnityEngine;

namespace CyberVeil.UI
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursor Textures")]
        [SerializeField] private Texture2D defaultCursor;
        [SerializeField] private Texture2D buttonHoverCursor;

        [Header("Hotspots")]
        [SerializeField] private Vector2 defaultHotspot = Vector2.zero;
        [SerializeField] private Vector2 buttonHotspot = Vector2.zero;

        [Header("Options")]
        [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetDefaultCursor();
        }

        public void SetDefaultCursor()
        {
            SetCursor(defaultCursor, defaultHotspot);
        }

        public void SetButtonHoverCursor()
        {
            SetCursor(buttonHoverCursor, buttonHotspot);
        }

        private void SetCursor(Texture2D texture, Vector2 hotspot)
        {
            if (texture == null)
            {
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                return;
            }

            Cursor.SetCursor(texture, hotspot, cursorMode);
        }
    }
}

using UnityEngine;

namespace CyberVeil.Systems
{
    /// <summary>
    /// Lightweight singleton that caches player reference once at startup
    /// Eliminates expensive FindGameObjectWithTag("Player") calls scattered throughout codebase
    /// All enemies and systems access cached reference via property (O(1) instead of O(n))
    /// </summary>
    public class PlayerReference : MonoBehaviour
    {
        public static Transform PlayerTransform { get; private set; }
        public static GameObject PlayerGameObject { get; private set; }

        private void Awake()
        {
            PlayerGameObject = gameObject;
            PlayerTransform = transform;
        }

        private void OnDestroy()
        {
            PlayerTransform = null;
            PlayerGameObject = null;
        }
    }
}

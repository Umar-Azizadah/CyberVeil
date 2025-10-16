// CyberVeil.UI
using TMPro;
using UnityEngine;

namespace CyberVeil.UI
{
    /// <summary>
    /// Displays a single line of dialogue and can be shown/hidden
    /// Driven by an interactable's coroutine to page through lines
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text body;

        public void ShowLine(string line) // Shows the dialogue UI and sets the given line of text
        {
            if (body) body.text = line;
            if (group) group.alpha = 1f;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (group) group.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}

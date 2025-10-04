// CyberVeil.UI
using TMPro;
using UnityEngine;

namespace CyberVeil.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text body;

        public void ShowLine(string line)
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

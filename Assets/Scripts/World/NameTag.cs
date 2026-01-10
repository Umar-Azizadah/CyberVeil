using TMPro;
using UnityEngine;

namespace CyberVeil.World
{
    /// <summary>
    /// Small world-space name tag for NPCs and object, Uses TextMeshPro 
    /// The name tag is inactive by default and can be shown via Show(true) when the player focuses the NPC/object
    /// </summary>
    [DisallowMultipleComponent]
    public class NameTag : MonoBehaviour
    {
        [SerializeField] private TextMeshPro nameText;

        private void Awake()
        {
            if (nameText == null) nameText = GetComponentInChildren<TextMeshPro>(true);
           
            if (nameText != null)
                nameText.gameObject.SetActive(false);
        }


        public void SetName(string displayName)
        {
            if (nameText != null) nameText.text = displayName;
        }

        public void Show(bool show)
        {
            if (nameText != null) nameText.gameObject.SetActive(show);
        }
    }
}

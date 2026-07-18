using UnityEngine;
using UnityEngine.EventSystems;
using CyberVeil.Systems;

namespace CyberVeil.UI
{
    /// <summary>
    /// Attach to a Button (or its parent) to play hover/click SFX using SoundManager
    /// </summary>
    public class PlaySoundOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("Minimum seconds between hover sound plays to avoid spam")]
        public float hoverCooldown = 0.08f;

        private float lastHoverTime = -10f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Time.realtimeSinceStartup - lastHoverTime < hoverCooldown) return;
            lastHoverTime = Time.realtimeSinceStartup;
            SoundManager.PlaySound(SoundType.CARDHOVER, 0.6f);
            SoundManager.PlaySound(SoundType.CARDCYBER, 0.6f);

            if (CursorManager.Instance != null)
                CursorManager.Instance.SetButtonHoverCursor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetDefaultCursor();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SoundManager.PlaySound(SoundType.CARDCLICK, 1f);
        }
    }
}

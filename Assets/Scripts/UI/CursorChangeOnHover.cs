using UnityEngine;
using UnityEngine.EventSystems;

namespace CyberVeil.UI
{
	public class CursorChangeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (CursorManager.Instance == null)
				return;

			CursorManager.Instance.SetButtonHoverCursor();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (CursorManager.Instance == null)
				return;

			CursorManager.Instance.SetDefaultCursor();
		}

		private void OnDisable()
		{
			if (CursorManager.Instance == null)
				return;

			CursorManager.Instance.SetDefaultCursor();
		}
	}
}

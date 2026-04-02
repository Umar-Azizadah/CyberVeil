using UnityEngine;
using UnityEngine.UI;

namespace CyberVeil.UI
{
    /// <summary>
    /// Subtle "energy flow" for UI by scrolling a RawImage's uvRect.
    ///
    /// Typical setup for a health bar:
    /// - Put this on a child RawImage inside your Fill Image.
    /// - Add a Mask component to the Fill Image and enable "Show Mask Graphic".
    ///   This clips the scrolling texture to the filled portion of the bar.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class UIRawImageUVScroller : MonoBehaviour
    {
        [SerializeField] private Vector2 uvSpeed = new Vector2(0.02f, 0f);
        [SerializeField] private Vector2 uvTiling = Vector2.one;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool randomizeStartOffset = true;

        private RawImage rawImage;
        private Vector2 offset;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();

            if (randomizeStartOffset)
                offset = new Vector2(Random.value, Random.value);
            else
                offset = rawImage.uvRect.position;

            ApplyUV();
        }

        private void Update()
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            offset += uvSpeed * dt;

            // Keep values bounded to avoid float growth
            offset.x = Mathf.Repeat(offset.x, 1f);
            offset.y = Mathf.Repeat(offset.y, 1f);

            ApplyUV();
        }

        private void ApplyUV()
        {
            if (rawImage == null) return;

            rawImage.uvRect = new Rect(offset.x, offset.y, uvTiling.x, uvTiling.y);
        }
    }
}

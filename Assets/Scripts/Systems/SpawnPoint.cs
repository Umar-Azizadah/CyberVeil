using UnityEngine;

namespace CyberVeil.Systems
{
    public enum SpawnLane
    {
        Any = 0,
        Left = 1,
        Right = 2,
        Front = 3,
        Back = 4,
        Center = 5
    }

    /// <summary>
    /// Optional marker for spawn points so waves can target lanes (left/right/front/back/etc.).
    /// If not set, the spawn point is treated as SpawnLane.Any.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpawnLane lane = SpawnLane.Any;
        public SpawnLane Lane => lane;
    }
}

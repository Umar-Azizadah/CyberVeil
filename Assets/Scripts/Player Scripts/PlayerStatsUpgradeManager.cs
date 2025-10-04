using System;
using UnityEngine;

namespace CyberVeil.Player
{
    /// <summary>
    /// Central manager for stat upgrades for the player
    /// The base stats (like default health, base damage, etc.) live in other scripts,
    /// at runtime, reads from this class to get upgraded values (damage, movement, dash distance, HP)
    /// </summary>
    public class PlayerStatsUpgradeManager : MonoBehaviour
    {
        public static PlayerStatsUpgradeManager Instance { get; private set; }

        [Header("Multipliers")]
        [Tooltip("1.0 = no change, 1.1 = +10% damage")]
        [SerializeField] private float damageMultiplier = 1f;

        [Header("Additive / Percent Bonuses")]
        [Tooltip("Extra fraction of base max health. 0.2 = +20% of base")]
        [SerializeField] private float maxHealthPct = 0f;
        [SerializeField] private float moveSpeedAdd = 0f;
        [SerializeField] private float dashDistanceAdd = 0f;
        
        /// <summary>
        /// Event triggered whenever any stat upgrade changes, other scrips subscribe to this
        /// to refresh derived values whenever an upgrade is applied.
        /// <summary>
        public event Action OnChanged;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ---------------- READ API -----------------
        // Properties are "read-only", clamped or returned safely to prevent invalid values
        public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);
        public float MaxHealthPct => Mathf.Max(0f, maxHealthPct);
        public float MoveSpeedAdd => moveSpeedAdd;
        public float DashDistanceAdd => dashDistanceAdd;

        // ---------------- WRITE API -----------------
        /// <summary>
        /// Increases multipliers, percenteges or flat bonuses by the specified amount and raises OnChanged
        /// </summary>
        public void AddDamageMultiplier(float add) { damageMultiplier += add; OnChanged?.Invoke(); }
        public void AddMaxHealthPercent(float addPct) { maxHealthPct += addPct; OnChanged?.Invoke(); }
        public void AddMoveSpeed(float add) { moveSpeedAdd += add; OnChanged?.Invoke(); }
        public void AddDashDistance(float add) { dashDistanceAdd += add; OnChanged?.Invoke(); }
    }
}

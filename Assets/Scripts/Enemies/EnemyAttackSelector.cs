using UnityEngine;
using System.Collections.Generic;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Selects and manages enemy attacks based on distance and cooldown
    /// Centralizes cooldown tracking for ScriptableObject-based attacks
    /// to enable reuse and decouple prefab lifecycle from timing logic
    /// </summary>
    public class EnemyAttackSelector : MonoBehaviour
    {
        [Tooltip("List of available attacks this enemy can perform")]
        [SerializeField] private List<EnemyAttackData> attacks;
        private float[] lastUsedTimestamps; // Stores the last-used timestamp per attack
        private Transform player;

        private EnemyAttackData selectedAttack; // Caches the most recently selected attack for execution and cooldown tracking

        /// <summary>
        /// Initializes the cooldown tracking array and caches the player reference
        /// This ensures that cooldown state persists across multiple attack prefab executions
        /// </summary>
        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            lastUsedTimestamps = new float[attacks.Count];
            for (int i = 0; i < attacks.Count; i++) lastUsedTimestamps[i] = -Mathf.Infinity; // Ensures all attacks are initially ready
        }

        /// <summary>
        /// Determines whether any attack is off cooldown and within usable range
        /// Also caches the selected attack for immediate use if one is ready
        /// </summary>
        public bool HasAttackReady()
        {
            float dist = Vector3.Distance(transform.position, player.position);
            for (int i = 0; i < attacks.Count; i++)
            {
                if (Time.time >= lastUsedTimestamps[i] + attacks[i].cooldown &&
                    dist <= attacks[i].attackRange)
                {
                    selectedAttack = attacks[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the previously selected attack (from HasAttackReady) and logs its use
        /// This allows cooldowns to be enforced without modifying the attack prefab itself
        /// </summary>
        public EnemyAttackData GetSelectedAttack()
        {
            if (selectedAttack != null)
            {
                int i = attacks.IndexOf(selectedAttack);
                if (i >= 0) lastUsedTimestamps[i] = Time.time;
            }
            return selectedAttack;
        }
    }
}

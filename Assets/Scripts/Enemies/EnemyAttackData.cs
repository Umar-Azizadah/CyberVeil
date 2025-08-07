using UnityEngine;

namespace CyberVeil.Enemies
{
    [CreateAssetMenu(menuName = "Enemy/Attack Data")]
    /// <summary>
    /// Represents reusable, data-driven attack configurations for enemies
    /// Encapsulates behavior parameters such as range, cooldown, and prefab reference
    /// enables modularity and scalability across different enemy types
    /// </summary>
    public class EnemyAttackData : ScriptableObject
    {
        public string attackName;
        public float attackRange; // Max range this attk can be triggered from
        public float cooldown;

        public GameObject attackPrefab; // prefab contatining an IEnemyAttack component to execute
    }
}

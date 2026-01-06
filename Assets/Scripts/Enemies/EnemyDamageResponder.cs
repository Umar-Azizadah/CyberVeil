using UnityEngine;
using CyberVeil.Combat;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Handles how the enemy responds to taking damage
    /// Bridges the health system to enemy AI logic, implements IDamageStateResponder to integrate with HealthComponent
    /// </summary>
    public class EnemyDamageResponder : MonoBehaviour, IDamageStateResponder
    {
        private EnemyAIController aiController;

        [Header("Stagger Settings")]
        [Range(0f,1f)]
        [Tooltip("Chance (0-1) that the enemy will stagger when damaged")] 
        public float staggerChance = 1f; // default: always stagger

        private void Start()
        {
            aiController = GetComponent<EnemyAIController>();
        }

        public void OnDamaged()
        {
            if (aiController != null)
            {
                // Roll to determine whether the enemy should enter the Damaged (stagger) state
                if (Random.value <= staggerChance)
                {
                    aiController.ChangeAIState(EnemyAIState.Damaged);
                }
                
            }
        }
    }
}

using CyberVeil.Core;
using UnityEngine;
using CyberVeil.Combat;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Handles how the enemy responds to taking damage
    /// Implements IDamageStateResponder to integrate with HealthComponent
    /// </summary>
    public class EnemyDamageResponder : MonoBehaviour, IDamageStateResponder
    {
        private EnemyAIController aiController;

        private void Start()
        {
            aiController = GetComponent<EnemyAIController>();
        }

        public void OnDamaged()
        {
            if (aiController != null)
            {
                aiController.ChangeAIState(EnemyAIState.Damaged);
            }
        }
    }
}

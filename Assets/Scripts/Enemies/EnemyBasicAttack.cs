using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.VFX;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Basic melee attack behavior for enemies
    /// Applies area damage to targets within radius after a delay
    /// Optionally plays a visual effect synced with the attack timing
    /// </summary>
    public class EnemyBasicAttack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private float attackRadius = 4f;
        [SerializeField] private float damageDelay = 1f; // Delay before damage is applied (synced with animation)e
        [SerializeField] private int damageAmount = 10;

        private IAttackEffect visualEffect;

        private void Start()
        {
            // Check for an optional VFX effect on this object or any child (some enemies keep VFX on a child object)
            visualEffect = GetComponent<IAttackEffect>() ?? GetComponentInChildren<IAttackEffect>();
        }

        /// <summary>
        /// Triggers the attack, plays visual (if available) and applies damage after delay
        /// </summary>
        public IEnumerator ExecuteAttack()
        {
            if (visualEffect != null)
            {
                yield return StartCoroutine(visualEffect.PerformEffect(damageDelay));
            }
            else
            {
                yield return new WaitForSeconds(damageDelay); // Fallback if no visual
            }

            // Applies damage to player
            CombatManager.Instance.DealDamageInRadius(transform.position, attackRadius, damageAmount, transform.root.gameObject);
        }

            // Animation events in Unity cannot directly start coroutines that return IEnumerator.
            // Use this wrapper if you want to call the attack from an AnimationEvent on the enemy's Animator.
            public void TriggerExecuteAttack()
            {
                StartCoroutine(ExecuteAttack());
            }
    }
}
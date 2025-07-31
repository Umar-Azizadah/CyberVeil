using UnityEngine;
using System.Collections;
using System;

namespace CyberVeil.Enemies
{
    [RequireComponent(typeof(EnemyAIController))]
    /// <summary>
    /// Handles the enemy's damaged state behavior, manages stagger timing and delegates post-damage control
    /// Locks the enemy in a stunned state for a set duration then calls a callback (onComplete) to return to normal behavior
    /// </summary>
    public class EnemyDamaged : MonoBehaviour
    {
        [Header("Damaged Settings")]
        [SerializeField] private float damagedDuration = 1f;
        public bool isStaggered = false;

        // Reference to the running stagger coroutine, so it can be stopped if needed
        private Coroutine staggerRoutine;

        /// <summary>
        /// Triggers the damage response behavior
        /// Locks the enemy into a staggered state, waits for a duration then executes the provided onComplete callback
        /// </summary>
        public void TriggerDamage(Action onComplete)
        {
            if (isStaggered) return;

            // Stops any currently running stagger coroutine (safety check)
            if (staggerRoutine != null)
                StopCoroutine(staggerRoutine);

            // Begin controlled damage state with delegated exit logic
            staggerRoutine = StartCoroutine(HandleDamage(onComplete));
        }

        // Executes timed lockout behavior, then triggers externally defined recovery logic
        private IEnumerator HandleDamage(Action onComplete)
        {
            isStaggered = true;

            // Wait for damage reaction
            yield return new WaitForSeconds(damagedDuration);

            isStaggered = false;
            onComplete?.Invoke();
        }
    }
}

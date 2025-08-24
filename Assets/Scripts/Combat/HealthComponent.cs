using CyberVeil.Audio;
using CyberVeil.Core;
using CyberVeil.VFX;
using UnityEngine;
using System;

namespace CyberVeil.Combat
{
    /// <summary>
    /// Handles health management for any damageable entity, including damage intake,
    /// faction identity, UI event broadcasting and integration with visual/audio feedback and death logic
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
         public Faction faction; // To determine which team each entity belongs to
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;

        public event Action<HealthComponent> OnHealthChanged; // Event for UI (or other systems) to suscribe to whenever health changes

        // Public readonly to expose private fields
        public float Normalized => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f; // Returns health as a percentage for use in UI

        private void Awake()
        {
            currentHealth = maxHealth; // Sets health to max at start      
        }

        private void OnEnable()
        {
            OnHealthChanged?.Invoke(this); // Broadcast initial state so UI is correct at game start
        }

        /// <summary>
        /// Applies damage to the entity, triggers optional feedback systems,
        /// and destroys the entity if health reaches zero or below
        /// </summary>
        /// <param name="damage">The amount of damage to apply.</param>
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            OnHealthChanged?.Invoke(this); // Broadcast new health state to UI

            /// <summary>
            /// Local feedback systems for effects (only cares about THIS) using null checks to prevent runtime crashes
            /// </summary>
            IDamageVisual damageVisual = GetComponent<IDamageVisual>();
            damageVisual?.PlayDamageEffect();

            IDamageSound damageSound = GetComponent<IDamageSound>();
            damageSound?.PlayDamageSound();

            IDamageStateResponder damageStateResponder = GetComponent<IDamageStateResponder>();
            damageStateResponder?.OnDamaged();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Destroy(gameObject); //simple death logic
        }
    }
}

using CyberVeil.Audio;
using CyberVeil.Core;
using CyberVeil.VFX;
using UnityEngine;
using System;
using CyberVeil.Player;

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
        public float Normalized => GetEffectiveMax() > 0 ? (float)currentHealth / GetEffectiveMax() : 0f; // Returns health as a percentage for use in UI

        private void Awake()
        {
           currentHealth = GetEffectiveMax();; // Sets health to max at start      
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
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, GetEffectiveMax());

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

        private int GetEffectiveMax()
        {
            // Only the PLAYER gets the bonus; others use base maxHealth unchanged
            if (faction != Faction.Player) return maxHealth;

            var mods = PlayerStatModifiers.Instance;
            float pct = mods ? mods.MaxHealthPct : 0f;   // e.g., 0.20f = +20%
            return Mathf.Max(1, Mathf.RoundToInt(maxHealth * (1f + pct)));
        }


        private void Die()
        {
            Destroy(gameObject); //simple death logic
        }
    }
}

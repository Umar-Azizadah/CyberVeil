using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.VFX;
using CyberVeil.Systems;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// GolemSlamAttack - heavy telegraphed slam AoE attack.
    /// Designed as a prefab-based IEnemyAttack (instantiate as a child of the enemy).
    /// Behavior:
    /// - Short telegraph (windup) with light VFX/sound
    /// - Play heavy slam VFX and SFX
    /// - Apply radial damage via CombatManager and rely on IKnockbackable for knockback
    /// - Clean up the attack prefab when done
    /// </summary>
    public class GolemSlamAttack : MonoBehaviour, IEnemyAttack
    {
        // Guards to prevent double-execution if the coroutine is started more than once
        private bool attackStarted = false;
        private bool damageApplied = false;

        [Header("Timing")]
        [SerializeField] private float windupTime = 0.9f; // telegraph before slam
        [SerializeField] private float postSlamRecover = 0.5f; // short buffer after slam before finishing

        [Header("Damage")]
        [SerializeField] private float slamRadius = 3f;
        [SerializeField] private int damageAmount = 45;

        [Header("Audio / VFX")]
        [SerializeField] private float telegraphVol = 0.05f;
        [SerializeField] private float slamVol = 0.5f;

        // ExecuteAttack coroutine called by EnemyAIController for prefab-based attacks
        public IEnumerator ExecuteAttack()
        {
            // Prevent double-start on the same instance
            if (attackStarted)
            {
                yield break;
            }
            attackStarted = true;

            // Determine the enemy (attacker) that spawned this prefab. The AIController parents the prefab to the enemy.
            Transform enemyTransform = transform.parent ?? transform.root;
            GameObject attacker = enemyTransform != null ? enemyTransform.gameObject : this.gameObject;

            // Telegraph: play a subtle effect and sound to warn the player
            // Use Teleport VFX as a short ground rumble/indicator; fall back gracefully if manager missing
            if (ParticleManager.Instance != null)
                ParticleManager.Instance.PlayEffect(VFXType.MushroomShieldParticle, enemyTransform.position, Quaternion.identity);

            SoundManager.PlaySound(SoundType.WINDUP, telegraphVol);

            // Slight pause to telegraph the slam
            yield return new WaitForSeconds(windupTime);

            // Slam: big sound + impact VFX
            // Ensure we only play the slam/impact and apply damage once per attack instance
            if (!damageApplied)
            {
                if (ParticleManager.Instance != null)
                    ParticleManager.Instance.PlayEffect(VFXType.GROUNDSLAM, enemyTransform.position, Quaternion.identity);
                yield return new WaitForSeconds(1.3f);
                damageApplied = true;

                SoundManager.PlaySound(SoundType.GROUNDSLAMNOISE, slamVol);
                

                // Wait a short moment after the slam sound before applying damage so the impact is felt after the audio cu

                // Apply radial damage around the enemy's feet
                CombatManager.Instance.DealDamageInRadius(enemyTransform.position, slamRadius, damageAmount, attacker);
            }

            // Small recovery so animation/VFX can finish before destroying prefab
            yield return new WaitForSeconds(postSlamRecover);

            // Clean up
            Destroy(gameObject);
        }
    }
}

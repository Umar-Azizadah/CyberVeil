using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.VFX;
using CyberVeil.Systems;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Leap (AoE) attack for enemies
    /// Moves the actual enemy using an arced trajectory, spawns impact effects,
    /// and applies radial damage on landing. Designed for use as a reusable attack prefab.
    /// </summary>
    public class LeapAttack : MonoBehaviour, IEnemyAttack
    {
        [Header("Leap Settings")]
        [SerializeField] private float leapForce = 5f;
        [SerializeField] private float leapHeight = 2f;
        [SerializeField] private float leapDuration = 0.4f;

         [Header("Optional VFX & Audio")]
        [SerializeField] private VFXType particleType = VFXType.SlimeSplat;
        [SerializeField] private SoundType audioType = SoundType.ATTACK;
        [SerializeField] private SoundType audioType2 = SoundType.ATTACK;
        [SerializeField] private float audioVolume = 0.4f;
        [SerializeField] private float audioVolume2 = 0.6f;
        [SerializeField] private float particleAndOrAudioPlayDelay = 0.3f;


        [Header("Attack Settings")]
        [SerializeField] private float damageRadius = 2f;
        [SerializeField] private float damageDelay = 1f; // Delay before damage is applied (synced with animation)e
        [SerializeField] private int damageAmount = 30;

        private Transform player;
        private Transform slimeTransform;

        /// <summary>
        /// Executes the leap attack behavior:
        /// </summary>
        public IEnumerator ExecuteAttack()
        {
            // Ensure player is assigned
            if (player == null)
                player = PlayerReference.PlayerTransform;

            // Get the enemy GameObject that spawned this attack prefab
            slimeTransform = transform.parent;
            if (slimeTransform == null)
            {
                yield break;
            }

            SoundManager.PlaySound(audioType, audioVolume);
            yield return new WaitForSeconds(particleAndOrAudioPlayDelay); // Delay before leap, gives player a chance to prepare 

            Vector3 start = slimeTransform.position;
            Vector3 direction = (player.position - slimeTransform.position).normalized;
            Vector3 target = start + direction * leapForce;

            float elapsed = 0f;

            // Perform arc-based leap over time
            while (elapsed < leapDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / leapDuration);

                Vector3 flat = Vector3.Lerp(start, target, t);
                float height = Mathf.Sin(t * Mathf.PI) * leapHeight;
                slimeTransform.position = new Vector3(flat.x, start.y + height, flat.z);

                yield return null;
            }

            // Snap to final landing position
            slimeTransform.position = new Vector3(target.x, start.y, target.z);

            // Impact sound and particle effects
            SoundManager.PlaySound(audioType2, audioVolume2);
            ParticleManager.Instance.PlayEffect(particleType, slimeTransform.position, Quaternion.identity);

            // AoE damage
            CombatManager.Instance.DealDamageInRadius(slimeTransform.position, damageRadius, damageAmount, slimeTransform.gameObject);

            // Cleans up the attack prefab instance
            Destroy(gameObject);
        }
    }
}

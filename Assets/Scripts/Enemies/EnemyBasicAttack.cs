using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.VFX;
using CyberVeil.Systems;

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

         [Header("Optional VFX & Audio")]
        [SerializeField] private bool playParticleOnStartAttack = false;
        [SerializeField] private bool playParticleOnEndAttack = false;
        [SerializeField] private VFXType particleType = VFXType.SlashHit;
        [SerializeField] private VFXType particleType2 = VFXType.SlashHit;
        [SerializeField] private Vector3 particleSpawnOffset = Vector3.zero;
        [SerializeField] private bool playAudioOnStartAttack = false;
        [SerializeField] private bool playAudioOnEndAttack = false;
        [SerializeField] private SoundType audioType = SoundType.ATTACK;
        [SerializeField] private SoundType audioType2 = SoundType.ATTACK;
        [SerializeField] private float audioVolume = 0.5f;
        [SerializeField] private float audioVolume2 = 0.5f;
        [SerializeField] private float particleAndOrAudioPlayDelay = 0.5f;
        [SerializeField] private float particleAndOrAudioPlayDelay2 = 0.5f;

        private IAttackEffect visualEffect;

        private void Start()
        {
            // Check for an optional VFX effect on this object or any child (some enemies keep VFX on a child object)
            visualEffect = GetComponent<IAttackEffect>() ?? GetComponentInChildren<IAttackEffect>();

            // Kick off delayed one-shot particle/audio at spawn if desired
            StartCoroutine(PlaySpawnEffectsAfterDelay());
        }

        private IEnumerator PlaySpawnEffectsAfterDelay()
        {
            if (particleAndOrAudioPlayDelay > 0f)
                yield return new WaitForSeconds(particleAndOrAudioPlayDelay);

            if (playAudioOnStartAttack && ParticleManager.Instance != null)
            {
                Vector3 spawnPos = transform.position + particleSpawnOffset;
                ParticleManager.Instance.PlayEffect(particleType, spawnPos, transform.rotation);
            }
            if (playAudioOnStartAttack)
            {
                SoundManager.PlaySound(audioType, audioVolume);
            }

            if (playAudioOnEndAttack)
            {
                yield return new WaitForSeconds(particleAndOrAudioPlayDelay2);
                SoundManager.PlaySound(audioType2, audioVolume2);
            }
            if (playAudioOnEndAttack && ParticleManager.Instance != null)
            {
                Vector3 spawnPos = transform.position + particleSpawnOffset;
                ParticleManager.Instance.PlayEffect(particleType2, spawnPos, transform.rotation);
            }
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

            // Animation events in Unity cannot directly start coroutines that return IEnumerator
            // Use this wrapper if you want to call the attack from an AnimationEvent on the enemy's Animator
            public void TriggerExecuteAttack()
            {
                StartCoroutine(ExecuteAttack());
            }
    }
}
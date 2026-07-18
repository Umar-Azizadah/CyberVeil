using System.Collections;
using UnityEngine;
using CyberVeil.Systems;
using CyberVeil.VFX;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Charge attack: lock player's position, telegraph, then dash in a straight line.
    /// If the player is outside missRadius at the end, the enemy staggers itself.
    /// </summary>
    public class ChargeAttack : MonoBehaviour, IEnemyAttack
    {
        [Header("Telegraph")]
        [SerializeField] private float telegraphSeconds = 0.6f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 8f;
        [SerializeField] private float maxDashSeconds = 0.6f;
        [SerializeField] private float maxDashDistance = 8f;
        [SerializeField] private float missRadius = 1.5f;

        [Header("Optional VFX & Audio")]
        [SerializeField] private VFXType particleType = VFXType.SlashHit;
        [SerializeField] private VFXType particleType2 = VFXType.SlashHit;
        [SerializeField] private Vector3 particleSpawnOffset = Vector3.zero;
        [SerializeField] private bool playAudioOnStartAttack = false;
        [SerializeField] private bool playAudioOnEndAttack = false;
        [SerializeField] private bool playSecondParticle = true;
        [SerializeField] private SoundType audioType = SoundType.ATTACK;
        [SerializeField] private SoundType audioType2 = SoundType.ATTACK;
        [SerializeField] private float audioVolume = 0.5f;
        [SerializeField] private float audioVolume2 = 0.5f;
        [SerializeField] private float particleAndOrAudioPlayDelay = 0.5f;
        [SerializeField] private float particleAndOrAudioPlayDelay2 = 0.5f;

        private Transform player;
        private Transform owner;
        private CharacterController controller;
        private EnemyDamaged damaged;
        private EnemyAIController ai;

        private void Awake()
        {
            owner = transform.parent != null ? transform.parent : transform;
            controller = owner.GetComponent<CharacterController>();
            damaged = owner.GetComponent<EnemyDamaged>();
            ai = owner.GetComponent<EnemyAIController>();
        }

        public IEnumerator ExecuteAttack()
        {
            if (player == null)
                player = PlayerReference.PlayerTransform;

            if (player == null || owner == null)
                yield break;

            Vector3 startPos = owner.position;
            Vector3 targetPos = player.position;
            Vector3 dir = targetPos - startPos;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                dir = owner.forward;
            else
                dir.Normalize();

            // Face the locked target direction during telegraph
            owner.rotation = Quaternion.LookRotation(dir, Vector3.up);

            StartCoroutine(PlaySpawnEffectsAfterDelay());

            if (telegraphSeconds > 0f)
                yield return new WaitForSeconds(telegraphSeconds);

            float elapsed = 0f;
            float maxSeconds = Mathf.Max(0.01f, maxDashSeconds);
            float maxDist = Mathf.Max(0.01f, maxDashDistance);
            float traveled = 0f;

            while (elapsed < maxSeconds && traveled < maxDist)
            {
                float step = dashSpeed * Time.deltaTime;
                Vector3 move = dir * step;
                Vector3 prev = owner.position;

                if (controller != null)
                    controller.Move(move);
                else
                    owner.position += move;

                traveled += (owner.position - prev).magnitude;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // If the player isn't near the locked target, self-stagger
            float missDist = Vector3.Distance(new Vector3(player.position.x, targetPos.y, player.position.z), targetPos);
            if (missDist > missRadius)
                TriggerSelfStagger();
        }

        private IEnumerator PlaySpawnEffectsAfterDelay()
        {
            if (particleAndOrAudioPlayDelay > 0f)
                yield return new WaitForSeconds(particleAndOrAudioPlayDelay);

            if (playAudioOnStartAttack && ParticleManager.Instance != null)
            {
                Vector3 spawnPos = owner.position + particleSpawnOffset;
                ParticleManager.Instance.PlayEffect(particleType, spawnPos, owner.rotation);
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
            if (playSecondParticle && ParticleManager.Instance != null)
            {
                Vector3 spawnPos = owner.position + particleSpawnOffset;
                ParticleManager.Instance.PlayEffect(particleType2, spawnPos, owner.rotation);
            }
        }

        private void TriggerSelfStagger()
        {
            if (ai != null)
                ai.ChangeAIState(EnemyAIState.Damaged);

            if (damaged != null)
            {
                damaged.TriggerDamage(() =>
                {
                    if (ai != null)
                        ai.ChangeAIState(EnemyAIState.Chase);
                });
            }
        }
    }
}

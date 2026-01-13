using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using CyberVeil.VFX;
using CyberVeil.Systems;
using CyberVeil.Core;

namespace CyberVeil.Player
{
    /// <summary>
    /// Offensive Boost Skill: Press E every 30 seconds to activate.
    /// 1.5 seconds: activation phase with particles, player frozen
    /// 5 seconds after: faster attack speed, attack locking disabled, player has dashdissolve effect
    /// </summary>
    public class VeilSurgeSkill : MonoBehaviour
    {
        [Header("Cooldown")]
        [SerializeField] private float skillCooldown = 30f;

        [Header("Activation Phase")]
        [SerializeField] private float activationDuration = 1.5f;
        [SerializeField] private VFXType activationParticleType = VFXType.VeilSurgeActivation;
        [SerializeField] private Vector3 particleOffset = Vector3.zero;

        [Header("Active Phase")]
        [SerializeField] private Material VeilSurgeMaterial;
        public PlayerParticles veilSurgeParticles;
        [SerializeField] private float activeDuration = 5f;
        [SerializeField] private float attackSpeedMultiplier = 2.5f; // 1.5x faster attacks

        [Header("Audio")]
        [SerializeField] private SoundType activationSound = SoundType.VEILSURGEACTIVATION;
        [Range(0f, 1f)] [SerializeField] private float activationVolume = 0.5f;
        [SerializeField] private SoundType activeSound = SoundType.VEILSURGEACTIVE;
        [Range(0f, 1f)] [SerializeField] private float activeVolume = 0.45f;

        // State
        private float lastSkillTime = -999f;
        private float activationStartTime = -999f;
        private bool isActivating = false;
        private bool isVeilSurge = false;


        private PlayerController playerController;
        private CharacterStateMachine stateMachine;
        private DissolveEffectHandler dissolveHandler;

        private void Start()
        {
            playerController = GetComponent<PlayerController>();
            stateMachine = GetComponent<CharacterStateMachine>();
            dissolveHandler = GetComponent<DissolveEffectHandler>();
        }

        private void Update()
        {
            // Check E key to activate (only if cooldown ready)
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (Time.time - lastSkillTime >= skillCooldown)
                {
                    ActivateSkill();
                }
            }

            // Update activation phase
            if (isActivating)
            {
                float elapsed = Time.time - activationStartTime;
                if (elapsed >= activationDuration)
                {
                    // Transition to offensive mode
                    isActivating = false;
                    isVeilSurge = true;
                    
                    // Re-enable movement
                    if (playerController != null)
                        playerController.canMove = true;

                    // Add particles and sound
                    veilSurgeParticles.ShowParticle();
                    SoundManager.PlaySound(activeSound,activeVolume);
                    // Change to skill material
                    if (dissolveHandler != null)
                    {
                        dissolveHandler.targetRenderer.material = VeilSurgeMaterial;
                        dissolveHandler.targetRenderer.material.SetFloat(dissolveHandler.dissolveProperty, 1f);
                    }
                }
            }

            // Update offensive mode
            if (isVeilSurge)
            {
                float elapsed = Time.time - activationStartTime;
                if (elapsed >= activationDuration + activeDuration)
                {
                    // End skill - return to base material
                    isVeilSurge = false;
                    veilSurgeParticles.HideParticle();
                    if (dissolveHandler != null)
                    {
                        dissolveHandler.targetRenderer.material = dissolveHandler.baseMaterial;
                        dissolveHandler.targetRenderer.material.SetFloat(dissolveHandler.dissolveProperty, 0f);
                    }
                }
            }
        }

        private void ActivateSkill()
        {
            lastSkillTime = Time.time;
            activationStartTime = Time.time;
            isActivating = true;

            // Disable movement
            if (playerController != null)
                playerController.canMove = false;

            // Play sound
            SoundManager.PlaySound(activationSound, activationVolume);

            // Spawn particles
            Vector3 spawnPos = transform.position + particleOffset;
            ParticleManager.Instance.PlayEffect(activationParticleType, spawnPos, transform.rotation);

            // Force idle
            if (stateMachine != null)
                stateMachine.ChangeState(CharacterState.Idle);
        }

        /// <summary>
        /// Check if skill is currently in offensive mode
        /// </summary>
        public bool IsVeilSurge => isVeilSurge;

        /// <summary>
        /// Get attack speed multiplier during offensive mode
        /// </summary>
        public float GetAttackSpeedMultiplier() => isVeilSurge ? attackSpeedMultiplier : 1f;

        /// <summary>
        /// Check if attack locking should be bypassed during offensive mode
        /// </summary>
        public bool ShouldBypassAttackLocking => isVeilSurge;
    }
}

using System;
using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.Enemies;
using CyberVeil.Player;

namespace CyberVeil.Systems
{
    /// <summary>
    /// Applies a random trial curse at the start of each trial and clears it at trial end.
    /// Hooks into WaveManager events and can drive UI through OnCurseApplied.
    /// </summary>
    public class TrialCurseModifier : MonoBehaviour
    {
        public enum CurseType
        {
            None = 0,
            DoubleEnemySpeed = 1,
            PlayerChipDamage = 2,
            ReducedAttackLimit = 3
        }

        public static event Action<CurseType, int> OnCurseApplied;
        public static event Action OnCurseCleared;

        public static float EnemySpeedMultiplier { get; private set; } = 1f;

        [Header("Curse Settings")]
        [SerializeField] private bool enableCurses = true;
        [Range(0f, 1f)]
        [SerializeField] private float curseChance = 0.6f;

        [Header("Double Speed")]
        [SerializeField] private float speedMultiplier = 2f;

        [Header("Chip Damage")]
        [SerializeField] private float chipDamagePerSecond = 2f;
        [SerializeField] private float chipTickSeconds = 1f;

        [Header("Reduced Attack Limit")]
        [SerializeField] private int reducedLimit = 3;

        [Header("Bonus")]
        [SerializeField] private int bonusUpgradeSlots = 1;

        private CurseType currentCurse = CurseType.None;
        private bool hadCurseThisRun = false;
        private bool skipRandomThisWave = false;
        private Coroutine chipRoutine;
        private AttackLimiterMechanic attackLimiter;
        private HealthComponent playerHealth;
        private float activeSpeedMultiplier = 1f;

        private void OnEnable()
        {
            WaveManager.OnWaveStarted += HandleWaveStarted;
            WaveManager.OnWaveCleared += HandleWaveCleared;
        }

        private void OnDisable()
        {
            WaveManager.OnWaveStarted -= HandleWaveStarted;
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            ClearCurse();
        }

        private void Start()
        {
            attackLimiter = FindObjectOfType<AttackLimiterMechanic>();
            playerHealth = FindPlayerHealth();
        }

        private void HandleWaveStarted(int trialIndex, int waveIndex)
        {
            if (skipRandomThisWave)
            {
                skipRandomThisWave = false;
                return;
            }

            ResolveCurse();
        }

        private void HandleWaveCleared(int trialIndex, int waveIndex)
        {
            ClearCurse();
        }

        public void ApplyForcedCurse(CurseType curse)
        {
            skipRandomThisWave = true;
            ClearCurse();

            if (!enableCurses)
                return;

            if (curse == CurseType.None)
                return;

            currentCurse = curse;
            hadCurseThisRun = true;
            ApplyCurse(currentCurse);
            OnCurseApplied?.Invoke(currentCurse, bonusUpgradeSlots);
        }

        private void ResolveCurse()
        {
            ClearCurse();

            if (!enableCurses)
                return;

            if (UnityEngine.Random.value > curseChance)
                return;

            CurseType[] options =
            {
                CurseType.DoubleEnemySpeed,
                CurseType.PlayerChipDamage,
                CurseType.ReducedAttackLimit
            };

            currentCurse = options[UnityEngine.Random.Range(0, options.Length)];
            hadCurseThisRun = true;
            ApplyCurse(currentCurse);
            OnCurseApplied?.Invoke(currentCurse, bonusUpgradeSlots);
        }

        private void ApplyCurse(CurseType curse)
        {
            switch (curse)
            {
                case CurseType.DoubleEnemySpeed:
                    ApplyDoubleEnemySpeed();
                    break;
                case CurseType.PlayerChipDamage:
                    ApplyChipDamage();
                    break;
                case CurseType.ReducedAttackLimit:
                    ApplyReducedAttackLimit();
                    break;
                default:
                    break;
            }
        }

        private void ApplyDoubleEnemySpeed()
        {
            activeSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            EnemySpeedMultiplier = activeSpeedMultiplier;

            // Apply to already spawned enemies (new ones will read the multiplier on Start)
            EnemyAIController[] enemies = FindObjectsOfType<EnemyAIController>();
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].speed *= activeSpeedMultiplier;
            }
        }

        private void ApplyChipDamage()
        {
            if (playerHealth == null)
                playerHealth = FindPlayerHealth();

            if (playerHealth == null)
                return;

            if (chipRoutine != null)
                StopCoroutine(chipRoutine);

            chipRoutine = StartCoroutine(ChipDamageLoop());
        }

        private IEnumerator ChipDamageLoop()
        {
            while (playerHealth != null)
            {
                yield return new WaitForSeconds(chipTickSeconds);
                int dmg = Mathf.Max(1, Mathf.RoundToInt(chipDamagePerSecond));
                playerHealth.TakeDamage(dmg);
            }
        }

        private void ApplyReducedAttackLimit()
        {
            if (attackLimiter != null)
            {
                attackLimiter.SetLimit(reducedLimit);
            }
        }

        public void ClearCurse()
        {
            if (currentCurse == CurseType.DoubleEnemySpeed && activeSpeedMultiplier > 1f)
            {
                EnemyAIController[] enemies = FindObjectsOfType<EnemyAIController>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    enemies[i].speed /= activeSpeedMultiplier;
                }
            }

            EnemySpeedMultiplier = 1f;
            activeSpeedMultiplier = 1f;

            if (chipRoutine != null)
            {
                StopCoroutine(chipRoutine);
                chipRoutine = null;
            }

            if (attackLimiter != null)
                attackLimiter.ResetLimit();

            currentCurse = CurseType.None;
            OnCurseCleared?.Invoke();
        }

        public CurseType GetCurrentCurse() => currentCurse;
        public int GetBonusUpgradeSlots() => bonusUpgradeSlots;
        public int GetRunBonusSlots() => hadCurseThisRun ? bonusUpgradeSlots : 0;
        public bool HasActiveCurse() => currentCurse != CurseType.None;

        public void ClearRunBonus()
        {
            hadCurseThisRun = false;
        }

        private static HealthComponent FindPlayerHealth()
        {
            HealthComponent[] all = FindObjectsOfType<HealthComponent>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].faction == CyberVeil.Core.Faction.Player)
                    return all[i];
            }
            return null;
        }
    }
}

using UnityEngine;
using System.Collections;
using System;
using CyberVeil.Enemies;
using CyberVeil.Systems;

namespace CyberVeil.Systems
{
    /// <summary>
    /// 3 trials x 3 wabes
    /// Manages the spawning and progression of enemy waves
    /// Waves auto chain EXCEPT after wave 3 of each trial, which triggers upgrade phase
    /// Handles wave definitions, spawn timing, and invokes events when waves start or end
    /// Supports randomized enemy types and spawn points for each wave
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [System.Serializable]
        public class Wave // Can edit waves in the unity editor
        {
            public string name;
            public GameObject[] enemyPrefabs;
            public int enemyCount;
            public float spawnRate = 1.5f;
        }

        [Header("Trials(3) - each trial has 3 waves")]
        public Wave[] trial1 = new Wave[3];
        public Wave[] trial2 = new Wave[3];
        public Wave[] trial3 = new Wave[3];

        public Transform[] spawnPoints;

        [Header("Flow")]
        public bool autoStartOnPlay = false;

        // State
        private int trialIndex = 0;
        private int waveIndex = 0;
        private bool waveInProgress = false;
        private bool waitingForUpgrade = false;

        private int aliveEnemies = 0; // Tracks actual alive enemies for the current user

        // Events for external systems (UI, music)
        public static event Action<int, int> OnWaveStarted;
        public static event Action<int, int> OnWaveCleared;
        public static event Action<int> OnUpgradePhaseStarted; // Fired after wave 3 cleared

        private void Start()
        {
            if (autoStartOnPlay) StartRun();
        }

        public void StartRun()
        {
            trialIndex = 0;
            waveIndex = 0;
            waitingForUpgrade = false;

            if (!waveInProgress) StartCoroutine(RunCurrentWave());
        }

        /// <summary>
        /// Calls this after the player uses teleporter upgrade UI, (in future: teleports them to next scene/level)
        /// Starts the next trials wave 1
        /// </summary>
        public void ContinueAfterUpgrade()
        {
            if (!waitingForUpgrade) return;

            waitingForUpgrade = false;

            // Move to next trial
            trialIndex++;
            waveIndex = 0;

            if (trialIndex >= 3)
            {
                Debug.Log("All trials complete"); // Will figure something out later
                return;
            }
        }

        private IEnumerator RunCurrentWave()
        {
            waveInProgress = true;
            aliveEnemies = 0;

            Wave wave = GetWave(trialIndex, waveIndex);
            if (wave == null)
            {
                Debug.Log($"Missing wave data for trial {trialIndex + 1}, wave {waveIndex + 1}");
                waveInProgress = false;
                yield break;
            }

            OnWaveStarted?.Invoke(trialIndex, waveIndex);

            // Spawn loop
            for (int i = 0; i < wave.enemyCount; i++)
            {
                GameObject enemyPrefab = wave.enemyPrefabs[UnityEngine.Random.Range(0, wave.enemyPrefabs.Length)];
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

                GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                aliveEnemies++;

                // Reporter attatchment so know when this enemy dies (Destroy)
                var reporter = enemyInstance.GetComponent<WaveEnemyReporter>();
                if (reporter == null) reporter = enemyInstance.AddComponent<WaveEnemyReporter>();
                reporter.Init(this);

                //Patrol assignment logic
                EnemyPatrol patrol = enemyInstance.GetComponent<EnemyPatrol>();
                if (patrol != null && spawnPoint.childCount > 0)
                {
                    Transform[] patrolPoints = new Transform[spawnPoint.childCount];
                    for (int j = 0; j < spawnPoint.childCount; j++)
                    {
                        patrolPoints[j] = spawnPoint.GetChild(j);
                    }
                    patrol.AssignPatrolPoints(patrolPoints);
                }

                yield return new WaitForSeconds(wave.spawnRate);
            }

            // Wait until all enemies are dead
            yield return new WaitUntil(() => aliveEnemies <= 0);

            // Wave cleared
            OnWaveCleared?.Invoke(trialIndex, waveIndex);

            // Decide what happens next
            bool isThirdWave = (waveIndex == 2);

            if (isThirdWave)
            {
                // Pause for upgrade teleporter
                waitingForUpgrade = true;
                SoundManager.PlaySound(SoundType.TRIALEND,0.7f);
                waveInProgress = false;

                OnUpgradePhaseStarted?.Invoke(trialIndex);
                yield break;
            }
            else
            {
                // Auto start next wave in same trial
                waveIndex++;
                waveInProgress = false;

                // Small delay between waves
                yield return new WaitForSeconds(1.0f);

                StartCoroutine(RunCurrentWave());
            }
        }

        private Wave GetWave(int tIndex, int wIndex)
        {
            Wave[] t = tIndex switch
            {
                0 => trial1,
                1 => trial2,
                2 => trial3,
                _ => null
            };

            if (t == null || wIndex < 0 || wIndex >= t.Length) return null;
            return t[wIndex];
        }

        public bool IsWaveInProgress() => waveInProgress;
        public bool IsWaitingForUpgrade() => waitingForUpgrade;

        // Called by reporter when an enemy is destroyed/dies
        public void NotifyEnemyDestroyed()
        {
            aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        }

        ///<summary>
        /// Notifies WaveManager when a spawned enemy GameObject is destroyed
        /// </summary>
        public class WaveEnemyReporter : MonoBehaviour
        {
            private WaveManager manager;
            private bool initialized = false;

            public void Init(WaveManager wm)
            {
                manager = wm;
                initialized = true;
            }

            private void OnDestroy()
            {
                if (!initialized) return;
                if (manager == null) return;

                manager.NotifyEnemyDestroyed();
            }
        }
    }
}

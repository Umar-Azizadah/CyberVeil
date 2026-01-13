using System.Collections;
using UnityEngine;
using CyberVeil.Systems;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// Handles spawn, death, and ambient sounds for enemies.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EnemyAudio : MonoBehaviour
    {
        [Header("One-shots")]
        public bool playSpawnSound = true;
        public SoundType spawnSound;
        public bool playDeathSound = true;
        public SoundType deathSound;
        [Range(0f, 1f)] public float spawnVol = 1f;
        [Range(0f, 1f)] public float deathVol = 1f;

        [Header("Ambient")]
        public SoundType[] ambientSounds;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;
        public float ambientMinInterval = 3f;
        public float ambientMaxInterval = 8f;
        public bool playSpawnOnEnable = true;
        public bool startAmbientOnEnable = true;

        private Coroutine ambientRoutine;

        private void OnEnable()
        {
            if (playSpawnOnEnable && playSpawnSound)
                SoundManager.PlaySound(spawnSound, spawnVol);

            if (startAmbientOnEnable && ambientSounds != null && ambientSounds.Length > 0)
                StartAmbient();
        }

        private void OnDisable()
        {
            StopAmbient();
        }

        public void PlaySpawn()
        {
            if (playSpawnSound)
                SoundManager.PlaySound(spawnSound, spawnVol);
        }

        public void PlayDeath()
        {
            if (playDeathSound)
                SoundManager.PlaySound(deathSound, deathVol);
        }

        public void StartAmbient()
        {
            if (ambientSounds == null || ambientSounds.Length == 0)
                return;

            if (ambientRoutine == null)
                ambientRoutine = StartCoroutine(AmbientLoop());
        }

        public void StopAmbient()
        {
            if (ambientRoutine != null)
            {
                StopCoroutine(ambientRoutine);
                ambientRoutine = null;
            }
        }

        private IEnumerator AmbientLoop()
        {
            while (true)
            {
                float wait = Random.Range(ambientMinInterval, ambientMaxInterval);
                yield return new WaitForSeconds(wait);

                if (ambientSounds != null && ambientSounds.Length > 0)
                {
                    var clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                    SoundManager.PlaySound(clip, ambientVolume);
                }
            }
        }
    }
}

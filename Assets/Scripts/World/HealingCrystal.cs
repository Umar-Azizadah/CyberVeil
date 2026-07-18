using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.Core;

namespace CyberVeil.World
{
    /// <summary>
    /// Healing crystal that floats and rotates in place.
    /// Flickers before disappearing and heals the player on contact.
    /// </summary>
    public class HealingCrystal : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private float floatFrequency = 1.5f;
        [SerializeField] private float spawnHeightOffset = 0.5f;

        [Header("Lifetime")]
        [SerializeField] private float lifetimeDuration = 8f;
        [SerializeField] private float flickerDuration = 0.5f;
        [SerializeField] private float flickerInterval = 0.08f;

        [Header("Healing")]
        [SerializeField] private float healPercent = 0.10f;
        [SerializeField] private float healCooldown = 0.5f;

        private Vector3 basePosition;
        private Renderer[] renderers;
        private float lastHealTime;
        private Coroutine flickerRoutine;
        private float spawnTime;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            basePosition = transform.position;
            basePosition.y += spawnHeightOffset;
            transform.position = basePosition;
        }

        private void OnEnable()
        {
            basePosition = transform.position;
            spawnTime = Time.time;
            lastHealTime = -Mathf.Infinity;
            
            StartCoroutine(LifetimeRoutine());
        }

        private void Update()
        {
            // Rotate
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Float
            float offset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = new Vector3(basePosition.x, basePosition.y + offset, basePosition.z);
        }

        private IEnumerator LifetimeRoutine()
        {
            float flickerStartTime = lifetimeDuration - flickerDuration;
            
            // Wait until it's time to flicker
            yield return new WaitForSeconds(Mathf.Max(0f, flickerStartTime));

            // Flicker before destruction
            if (flickerDuration > 0f && flickerInterval > 0f)
            {
                if (flickerRoutine != null)
                    StopCoroutine(flickerRoutine);

                flickerRoutine = StartCoroutine(Flicker());
                yield return flickerRoutine;
            }

            // Destroy
            Destroy(gameObject);
        }

        private IEnumerator Flicker()
        {
            float elapsed = 0f;
            bool visible = false;

            while (elapsed < flickerDuration)
            {
                SetRenderersVisible(visible);
                visible = !visible;
                elapsed += flickerInterval;
                yield return new WaitForSeconds(flickerInterval);
            }

            SetRenderersVisible(true);
        }

        private void SetRenderersVisible(bool visible)
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - lastHealTime < healCooldown)
                return;

            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null)
                return;

            if (health.faction != Faction.Player)
                return;

            health.HealPercent(healPercent);
            lastHealTime = Time.time;
            Destroy(gameObject);
        }
    }
}

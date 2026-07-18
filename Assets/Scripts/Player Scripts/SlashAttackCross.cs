using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.Systems;
using CyberVeil.VFX;

namespace CyberVeil.Player
{
    public class SlashAttackCross : MonoBehaviour
    {
        // Stores player transform
        public Transform playerTransform;
        public Vector3 offset = new Vector3(0f, 0f, 0f);
        [SerializeField] private float forwardDistance = 1.2f;
        [SerializeField] private Vector3 rotationOffsetA = new Vector3(0f, 0f, 45f);
        [SerializeField] private Vector3 rotationOffsetB = new Vector3(0f, 0f, -45f);

        // Reference to the new attack component
        public PlayerAttack playerAttack;
        private VeilSurgeSkill veilSurgeSkill;

        [Header("VFX")]
        [SerializeField] private VFXType slashType = VFXType.Slash2;
        [SerializeField] private VFXType surgeSlashType = VFXType.SurgeSlash2;

        [Header("Hit Feedback")]
        [SerializeField] private float hitStopDuration = 0.02f;

        private void Start()
        {
            // Auto-find PlayerAttack if not assigned
            if (playerAttack == null)
            {
                playerAttack = GetComponent<PlayerAttack>();
            }

            // Auto-find VeilSurgeSkill - search in parent or root player
            if (veilSurgeSkill == null)
            {
                veilSurgeSkill = GetComponentInParent<VeilSurgeSkill>();
            }
            if (veilSurgeSkill == null)
            {
                veilSurgeSkill = FindObjectOfType<VeilSurgeSkill>();
            }
        }

        public void PlaySlash(Vector3 forwardDir)
        {
            if (playerTransform == null)
                return;

            Vector3 slashPosition = playerTransform.position + forwardDir * forwardDistance + offset;
            Quaternion baseRotation = playerTransform.rotation;

            Quaternion rotationA = baseRotation * Quaternion.Euler(rotationOffsetA);
            Quaternion rotationB = baseRotation * Quaternion.Euler(rotationOffsetB);

            VFXType type = slashType;
            if (veilSurgeSkill != null && veilSurgeSkill.IsVeilSurge)
            {
                type = surgeSlashType;
            }

            ParticleManager.Instance.PlayEffect(type, slashPosition, rotationA);
            ParticleManager.Instance.PlayEffect(type, slashPosition, rotationB);
            StartCoroutine(ToggleCollider());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Vector3 hitPos = other.transform.position + Vector3.up * 1f;

                ParticleManager.Instance.PlayEffect(VFXType.SlashHit, hitPos, Quaternion.identity);
                ParticleManager.Instance.PlayEffect(VFXType.SlashImpact, hitPos, Quaternion.identity);

                if (hitStopDuration > 0f)
                    HitstopManager.Instance.DoHitstop(hitStopDuration, 0f);
            }
        }

        private IEnumerator ToggleCollider()
        {
            Collider collider = GetComponent<Collider>();
            if (collider == null)
                yield break;

            collider.enabled = true;
            yield return new WaitForSeconds(0.3f);
            collider.enabled = false;
        }
    }
}

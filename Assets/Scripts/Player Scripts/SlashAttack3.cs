using System.Collections;
using UnityEngine;
using CyberVeil.Combat;
using CyberVeil.Systems;
using CyberVeil.VFX;

namespace CyberVeil.Player
{
    public class SlashAttack3 : MonoBehaviour
    {
        //stores player transform
        public Transform playerTransform;
        public Vector3 offset = new Vector3(0, 0.0f, 0);
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;
        
        //reference player controller
        public PlayerAttack playerAttack;
        private VeilSurgeSkill veilSurgeSkill;

        void Start()
        {
            //auto-finds PlayerAttack if not assigned
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
            Vector3 slashPosition = playerTransform.position + offset;
            Quaternion slashRotation = playerTransform.rotation * Quaternion.Euler(rotationOffset);

            transform.position = slashPosition;
            transform.rotation = slashRotation;

            // Check if VeilSurge is active and play surge slash instead
            VFXType slashType = VFXType.Slash3;
            if (veilSurgeSkill != null && veilSurgeSkill.IsVeilSurge)
            {
                slashType = VFXType.SurgeSlash3;
            }

            ParticleManager.Instance.PlayEffect(slashType, slashPosition, slashRotation);
            ParticleManager.Instance.PlayEffect(slashType, slashPosition, slashRotation);
            StartCoroutine(toggleCollider());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                // Position above enemy (slightly raised for visibility)
                Vector3 hitPos = other.transform.position + Vector3.up * 1f;

                // Play pooled hit effects, now facing outward
                ParticleManager.Instance.PlayEffect(VFXType.SlashHit, hitPos, Quaternion.identity);
                ParticleManager.Instance.PlayEffect(VFXType.SlashImpact, hitPos, Quaternion.identity);

                //apply hit stop
                HitstopManager.Instance.DoHitstop(0.04f, 0f); //adjust duration & freeze level

            }
        }

        private IEnumerator toggleCollider()
        {
            Collider collider = GetComponent<Collider>();
            collider.enabled = true;
            yield return new WaitForSeconds(0.3f);
            collider.enabled = false;
        }

    }
}

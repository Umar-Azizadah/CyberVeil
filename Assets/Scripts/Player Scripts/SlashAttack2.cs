using UnityEngine;
using System.Collections;
using CyberVeil.Combat;
using CyberVeil.Systems;
using CyberVeil.VFX;


namespace CyberVeil.Player
{
    public class SlashAttack2 : MonoBehaviour
    {
        //stores player transform
        public Transform playerTransform;
        public Vector3 offset = new Vector3(0.5f, 0.5f, 10f);

        public float xRotate = 90f;
        public float yRotate = 45f;
        public float zRotate = 45f;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;
        
        //reference to the new attack component
        public PlayerAttack playerAttack;
        private VeilSurgeSkill veilSurgeSkill;


        void Start()
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
            // Position in front of player + offset
            Vector3 slashPosition = playerTransform.position + forwardDir * 1.2f + offset;
            // Base rotation facing forward
            Quaternion baseRotation = playerTransform.rotation;

            // Extra rotation (applied as a rotation offset)
            Quaternion customOffset = Quaternion.Euler(xRotate, yRotate, zRotate) * Quaternion.Euler(rotationOffset);

            Quaternion finalRotation = baseRotation * customOffset;

            // Check if VeilSurge is active and play surge slash instead
            VFXType slashType = VFXType.Slash2;
            if (veilSurgeSkill != null && veilSurgeSkill.IsVeilSurge)
            {
                slashType = VFXType.SurgeSlash2;
            }

            // Sets slash effect position and rotation via ParticleManager
            ParticleManager.Instance.PlayEffect(slashType, slashPosition, finalRotation);
            ParticleManager.Instance.PlayEffect(slashType, slashPosition, finalRotation);
            StartCoroutine(toggleCollider());
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("MEOW2");
                // Position above enemy (slightly raised for visibility)
                Vector3 hitPos = other.transform.position + Vector3.up * 1f;

                // Play pooled hit effects, now facing outward
                ParticleManager.Instance.PlayEffect(VFXType.SlashHit, hitPos, Quaternion.identity);
                ParticleManager.Instance.PlayEffect(VFXType.SlashImpact, hitPos, Quaternion.identity);
                // Apply hit pause
                HitstopManager.Instance.DoHitstop(0.01f, 0f); //adjust duration & freeze level

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

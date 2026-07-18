using UnityEngine;
using System.Collections.Generic;
using CyberVeil.Systems;
namespace CyberVeil.Enemies
{
    /// <summary>
    /// Controls enemy chasing behavior when in the Chase AI state
    /// Moves the enemy toward the player and smoothly rotates to face them
    /// </summary>
    [RequireComponent(typeof(EnemyAIController))]
    public class EnemyChase : MonoBehaviour
    {
        [Header("Chase Settings")]
        public float chaseSpeed = 1.8f;
        public float rotationSpeed = 8f;

        [Header("Flank Settings")]
        [SerializeField] private float flankCheckRadius = 4.5f;
        [SerializeField] private float flankSpacing = 1.5f;
        [SerializeField] private int flankMinChasers = 2;

        private EnemyAIController enemyAI;
        private Transform player;
        private readonly List<EnemyAIController> chasePack = new List<EnemyAIController>();

        private void Start()
        {
            enemyAI = GetComponent<EnemyAIController>();
            player = PlayerReference.PlayerTransform;
        }

        private void Update()
        {
            if (enemyAI.currentAIState != EnemyAIState.Chase || player == null) return;

            Vector3 toPlayer = (player.position - transform.position).normalized;
            Vector3 targetPos = player.position;

            int packCount = BuildChasePack();
            if (packCount >= flankMinChasers)
            {
                chasePack.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
                int index = chasePack.IndexOf(enemyAI);
                if (index >= 0)
                {
                    float centerOffset = index - (packCount - 1) * 0.5f;
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, toPlayer);
                    targetPos = player.position + perpendicular * (centerOffset * flankSpacing);
                }
            }

            // Move toward target position
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 move = new Vector3(direction.x, 0f, direction.z);
            float speedMultiplier = enemyAI != null ? enemyAI.speed : 1f;
            transform.position += move * chaseSpeed * speedMultiplier * Time.deltaTime;

            // Smooth rotation toward the player
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }

        private int BuildChasePack()
        {
            chasePack.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, flankCheckRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyAIController ai = hits[i].GetComponentInParent<EnemyAIController>();
                if (ai != null && ai.currentAIState == EnemyAIState.Chase && !chasePack.Contains(ai))
                    chasePack.Add(ai);
            }

            if (!chasePack.Contains(enemyAI) && enemyAI != null)
                chasePack.Add(enemyAI);

            return chasePack.Count;
        }
    }
}

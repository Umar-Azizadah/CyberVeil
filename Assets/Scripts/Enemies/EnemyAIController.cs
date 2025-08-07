using UnityEngine;
using CyberVeil.Core;
using System.Collections;

namespace CyberVeil.Enemies
{
    /// <summary>
    /// AI state controller (central brain) for enemy behavior, 
    /// controls high-level enemy AI behavior by managing movement, targeting, and state transitions
    /// Supports a modular, multi attack system via the EnemyAttackSelector component and ScriptableObject based attack data
    /// Delegates movement and animation control through CharacterStateMachine
    /// </summary>
    public class EnemyAIController : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 1f;

        [Header("Ranges")]
        public float attackRange = 2f;
        public float detectionRange = 4f;

        private float attackCooldown = 1.5f;
        public float lastAttackTime = -999f;
        private float waitStartTime = -1f;
        private float waitDuration = 0.7f;

        [Header("References")]
        private CharacterStateMachine characterStateMachine;
        private EnemyPatrol patrolBehavior;
        private EnemyChase chaseBehavior;
        private EnemyDamaged damagedBehavior;
        private Transform player;
        public EnemyAIState currentAIState = EnemyAIState.Idle;
        private EnemyAttackSelector attackSelector;
        private EnemyAttackData currentAttack;

        /// <summary>
        /// Caches all required references and components at runtime
        /// </summary>
        private void Start()
        {
            characterStateMachine = GetComponent<CharacterStateMachine>();
            patrolBehavior = GetComponent<EnemyPatrol>();
            chaseBehavior = GetComponent<EnemyChase>();
            damagedBehavior = GetComponent<EnemyDamaged>();
            attackSelector = GetComponent<EnemyAttackSelector>();

            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        /// <summary>
        /// Main decision making loop for AI behavior
        /// Transitions states based on player distance and internal timers
        /// Integrated with attack selection logic
        /// </summary>
        void Update()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            switch (currentAIState)
            {
                case EnemyAIState.Idle:
                    // If patrol exists, begin patrolling
                    // Otherwise, enter chase state if player is within detection range
                    if (patrolBehavior != null)
                        ChangeAIState(EnemyAIState.Patrol);
                    else if (distance < detectionRange)
                        ChangeAIState(EnemyAIState.Chase);
                    break;

                case EnemyAIState.Patrol:
                    characterStateMachine.ChangeState(CharacterState.Moving);
                    // Idle if waiting at patrol point
                    if (patrolBehavior.waiting)
                        characterStateMachine.ChangeState(CharacterState.Idle);
                    // Transition to Chase if player is nearby
                    if (distance < detectionRange)
                        ChangeAIState(EnemyAIState.Chase);
                    break;

                case EnemyAIState.Chase:
                    // Actively pursue the player while in detection range
                    // If an attack is available based on range and cooldown, transition to attack
                    characterStateMachine.ChangeState(CharacterState.Moving);
                    if (attackSelector.HasAttackReady())
                        ChangeAIState(EnemyAIState.Attack);
                    break;

                case EnemyAIState.Attack:
                    // Executes the selected attack if cooldown has elapsed
                    // The AttackSelector chooses the most appropriate attack (range + cooldown)
                    if (Time.time - lastAttackTime > attackCooldown)
                    {
                        lastAttackTime = Time.time;
                        currentAttack = attackSelector.GetSelectedAttack();
                        // Begins the attack and transitions to Wait once it completes (prevents spam attack)
                        StartCoroutine(HandleAttack(() => ChangeAIState(EnemyAIState.Wait)));
                    }
                    break;

                case EnemyAIState.Wait:
                    characterStateMachine.ChangeState(CharacterState.Idle);
                    // Brief delay after an attack before evaluating next action
                    // Returns to Chase if player has moved away
                    // Otherwise, rechecks cooldown availability before attacking again
                    if (distance > 1.5f)
                        ChangeAIState(EnemyAIState.Chase);
                    else if (Time.time - waitStartTime > waitDuration && attackSelector.HasAttackReady())
                        ChangeAIState(EnemyAIState.Attack);
                    break;

                case EnemyAIState.Damaged:
                    // Enemy is temporarily staggered from taking damage
                    // Once stagger ends, resume chasing the player
                    characterStateMachine.ChangeState(CharacterState.Damaged);
                    if (!damagedBehavior.isStaggered)
                    {
                        // Triggers the damage coroutine, when its done changes state to chase
                        damagedBehavior.TriggerDamage(() =>
                        {
                            ChangeAIState(EnemyAIState.Chase); // return to Chase after damage done
                        });
                    }
                    break;
            }
        }

        /// <summary>
        /// Executes the selected attack by instantiating a prefab with an IEnemyAttack implementation,
        /// or falls back to internal logic if no prefab is assigned
        /// </summary>
        private IEnumerator HandleAttack(System.Action onAttackComplete)
        {
            characterStateMachine.ChangeState(CharacterState.Attacking);

            if (currentAttack != null)
            {
                if (currentAttack.attackPrefab != null)
                {
                    // Prefab-based attack (used by most enemies)
                    GameObject attackInstance = Instantiate(currentAttack.attackPrefab, transform.position, Quaternion.identity, transform);
                    IEnemyAttack attackLogic = attackInstance.GetComponent<IEnemyAttack>();
                    if (attackLogic != null)
                        yield return StartCoroutine(attackLogic.ExecuteAttack());

                    Destroy(attackInstance);
                }
                else
                {
                    // Internal attack (used by Mushroom with EnemyBasicAttack + MushroomShieldAttack)
                    IEnemyAttack internalAttack = GetComponent<IEnemyAttack>();
                    if (internalAttack != null)
                        yield return StartCoroutine(internalAttack.ExecuteAttack());
                }
            }

            onAttackComplete?.Invoke();
        }

        /// <summary>
        /// Changes the enemy's current AI state
        /// Used to control behavior transitions in Update loop
        /// </summary>
        public void ChangeAIState(EnemyAIState newState)
        {
            if (newState == EnemyAIState.Wait)
                waitStartTime = Time.time;

            currentAIState = newState;
        }

    }
}
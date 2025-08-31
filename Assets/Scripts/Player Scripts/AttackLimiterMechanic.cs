using CyberVeil.Core;
using UnityEngine;
namespace CyberVeil.Player
{
    /// <summary>
    /// Enforces a cap on consecutive player attacks until a dash occurs (kind of like a stamina mechanic but with a twist)
    /// Implements <see cref="IAttackGate"/> so the combat system can query whether
    /// a new attack may start and notify the gate whenever an attack is performed
    /// So everytime the player does 4 swings they have to dash to unlock attacking, this adds a unique twist and helps balancing
    /// </summary>
    public class AttackLimiterMechanic : MonoBehaviour, IAttackGate
    {
        [SerializeField] private int limit = 5; // Max nunmber of consecutive attacks before lock

        private int count = 0;
        private CharacterStateMachine fsm; // Reference to finate state machine used to listen for dash transitions

        // Public read onlys
        public bool CanStartAttack => count < limit;
        public int CurrentCount => count;
        public int Limit => limit;
        public bool IsLocked => count >= limit;

        private void Awake()
        {
            fsm = GetComponent<CharacterStateMachine>();
            if (fsm != null)
                fsm.OnStateChange += OnStateChange; // Subscribe OnStateChange method to fsm state change event 
        }
        
        private void OnDestroy()
        {    
            if (fsm != null)
            {
                fsm.OnStateChange -= OnStateChange; // Unsubscribe to avoid memory leaks
            }
        }

        /// <summary>
        /// Handles state transitions and resets the attack counter according to the configured dash reset policy
        /// </summary>
        private void OnStateChange(CharacterState state)
        {
            if (state == CharacterState.Dashing)
            {
                ResetGate();
            }
        }

        // Called by attack system each time an attack actually begins
        public void RecordAttack()
        {
            if (count < limit)
            {
                count++;
            }
        }
        
        //Public reset so other systems can clear the counter
        //(in this case the dash logic)
        public void ResetGate()
        {
            count = 0; // Unlocks attacks again
        }
    }

}

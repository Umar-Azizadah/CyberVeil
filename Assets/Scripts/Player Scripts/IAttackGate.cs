using UnityEngine;
namespace CyberVeil.Player
{
    /// <summary>
    /// Contract for gating player attack actions
    /// This interface allows different systems (such as the attack limiter or other future systems)
    /// to consistently expose whether an attack can begin, how to record attacks,
    /// and how to reset the gating logic
    /// </summary>
    public interface IAttackGate
    {
        bool CanStartAttack {  get; }
        void RecordAttack(); // Increment logic
        void ResetGate();
        int Limit { get; }
        bool IsLocked { get; }

    }
}
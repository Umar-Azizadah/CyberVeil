using UnityEngine;


namespace CyberVeil.Core
{
    /// <summary>
    /// Contract for any world object the player can interact with via the Interact key
    /// </summary>
    public interface IInteractable
    {
        // A short prompt shown to the player when in range and aimed at the object (e.g., "Talk", "Open", "Upgrade")
        string Prompt { get; }

        // Called by the player's interactor when the player presses the Interact key
        void Interact(IInteractor interactor);
        void OnFocus(IInteractor interactor);
        void OnDefocus(IInteractor interactor);
    }

    /// <summary>
    /// Narrow interface the player exposes to interactables (prevents tight coupling).
    /// </summary>
    public interface IInteractor
    {
        Transform Transform { get; }
    }
}
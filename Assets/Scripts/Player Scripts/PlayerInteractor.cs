using System.Linq;
using CyberVeil.Core;
using UnityEngine;

namespace CyberVeil.Player
{
    /// <summary>
    /// Proximity-based interactor: finds the nearest IInteractable within a radius around the player
    /// No camera aiming required
    /// </summary>
    public class PlayerInteractor : MonoBehaviour, IInteractor
    {
        /// <summary>
        /// Layer mask for colliders that can be targeted as interactables
        /// </summary>
        [Header("Detection")]
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private float detectRadius = 0.5f; // Radius used to acquire a new target when none is currently focused
        [SerializeField] private float exitRadius = 0.8f; // Hysteresis so target doesn't flicker
        [SerializeField] private float retargetCooldown = 0.08f; // How often refresh (seconds)

        /// <summary>
        /// UI component responsible for showing/hiding the interact prompt
        /// </summary>
        [Header("UI")]
        [SerializeField] private UI.InteractPromptUI promptUI;
        [SerializeField] private string keyGlyph = "E";

        private IInteractable current; // The currently focused interactable (or null if none)
        private float nextProbe; // Timestamp for when the next retarget probe is allowed
        public Transform Transform => transform;

        /// <summary>
        /// Probes for the nearest interactable on a cadence, and handles input
        /// </summary>
        private void Update()
        {
            ProbeNearest();
            HandleInput();
        }

        /// <summary>
        /// Scans nearby colliders for a valid "IInteractable", and manages focus transitions 
        /// (OnFocus/OnDefocus) and prompt UI
        /// </summary>
        private void ProbeNearest()
        {
            if (Time.unscaledTime < nextProbe) return;
            nextProbe = Time.unscaledTime + retargetCooldown;

            // Choose radius based on whether already have a target (hysteresis)
            float radius = current == null ? detectRadius : exitRadius;

            Collider[] hits = Physics.OverlapSphere(transform.position, radius, interactableLayers, QueryTriggerInteraction.Collide);

            // Pick the nearest collider that belongs to an interactable
            IInteractable nearest = null;
            float bestSqr = float.PositiveInfinity;

            // Loops through every collider found in the sphere
            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                var interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null) continue;

                // Distance from player to the collider's closest surface
                Vector3 closest = col.ClosestPoint(transform.position);
                float sqr = (closest - transform.position).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = interactable;
                }
            }

            if (!ReferenceEquals(nearest, current))
            {
                // Defocus old
                current?.OnDefocus(this);

                // Set new
                current = nearest;

                if (current != null)
                {
                    current.OnFocus(this);
                    promptUI?.Show($"[{keyGlyph}] {current.Prompt}");
                }
                else
                {
                    promptUI?.Hide();
                }
            }
            else
            {
                // If we still have a target and it's inside detectRadius, make sure the prompt stays visible
                if (current != null && promptUI != null)
                    promptUI.Show($"[{keyGlyph}] {current.Prompt}");
            }
        }

        /// <summary>
        /// Handles user input for interaction, invokes "IInteractable.Interact"on key press
        /// </summary>
        private void HandleInput()
        {
            if (current == null) return;
            if (Input.GetKeyDown(KeyCode.E))
            {
                current.Interact(this);
            }
        }

        /// <summary>
        /// Hides the interact prompt
        /// </summary>
        public void HidePrompt()
        {
            promptUI?.Hide();
        }

        /// <summary>
        /// Shows the interact prompt for a specific target using its "IInteractable.Prompt"
        /// </summary>
        public void ShowPrompt(IInteractable target)
        {
            promptUI?.Show(target.Prompt);
        }

    }
}

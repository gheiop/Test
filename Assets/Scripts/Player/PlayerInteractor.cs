using UnityEngine;
using Islebound.Core;
using Islebound.Items;

namespace Islebound.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask interactMask;

        private PlayerDebugSettings debugSettings;
        private IInteractable currentInteractable;

        private void Awake()
        {
            debugSettings = GetComponent<PlayerDebugSettings>();
        }

        private void Update()
        {
            ScanForInteractable();
        }

        private void ScanForInteractable()
        {
            currentInteractable = null;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, GetInteractDistance(), interactMask, QueryTriggerInteraction.Ignore))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                    GameEvents.OnInteractionPromptShown?.Invoke(interactable.GetInteractionText());
                    return;
                }
            }

            GameEvents.OnInteractionPromptHidden?.Invoke();
        }

        public void TryInteract()
        {
            currentInteractable?.Interact();
        }

        private float GetInteractDistance()
        {
            return debugSettings != null && debugSettings.UseLiveTuning
                ? debugSettings.InteractDistance
                : interactDistance;
        }
    }
}
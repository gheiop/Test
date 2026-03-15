namespace Islebound.Items
{
    public interface IInteractable
    {
        string GetInteractionText();
        void Interact();
    }
}
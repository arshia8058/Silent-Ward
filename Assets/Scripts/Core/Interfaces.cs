namespace SilentWard.Core
{
    /// <summary>Anything the player can interact with (doors, documents, panels, items).</summary>
    public interface IInteractable
    {
        string InteractionKey { get; } // localization key for the prompt, e.g. "interact.read"
        bool CanInteract { get; }
        void Interact();
    }
}

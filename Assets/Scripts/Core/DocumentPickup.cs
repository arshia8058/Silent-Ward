using UnityEngine;
using SilentWard.Chapter01;
using SilentWard.Core;
using SilentWard.Dialogue;
using SilentWard.Inventory;

namespace SilentWard.Core
{
    /// <summary>Readable document / pickable item in the world (wristband, admission
    /// form, nurse's note...). Reading adds the document id to the save data,
    /// optionally adds an inventory item, plays its Chapter 1 dialogue once,
    /// and notifies Chapter01Controller so the chapter stage can advance.</summary>
    public class DocumentPickup : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        [SerializeField] private string documentId;      // e.g. "wristband", "admission_form", "nurse_note"
        [SerializeField] private string dialogueId;      // Chapter01Dialogue id, or empty for silent pickup
        [SerializeField] private string itemId;          // InventorySystem catalog id, or empty
        [SerializeField] private string promptKey = "interact.read";
        [SerializeField] private bool destroyAfterRead = true;

        private bool wasRead;

        public string InteractionKey => promptKey;
        public bool CanInteract => !wasRead;

        public void Interact()
        {
            if (wasRead) return;
            wasRead = true;

            var gm = GameManager.Instance;
            if (gm != null && !gm.CurrentData.collectedDocuments.Contains(documentId))
                gm.CurrentData.collectedDocuments.Add(documentId);

            if (!string.IsNullOrEmpty(itemId))
            {
                var inv = FindObjectOfType<InventorySystem>();
                inv?.Add(itemId);
            }

            var chapter = FindObjectOfType<Chapter01Controller>();
            chapter?.NotifyDocumentRead(documentId);

            var dialogue = FindObjectOfType<DialogueSystem>();
            if (!string.IsNullOrEmpty(dialogueId))
                Chapter01Dialogue.PlayDialogue(dialogue, dialogueId);

            if (destroyAfterRead) gameObject.SetActive(false);
        }
    }

    /// <summary>Autosave checkpoint zone (GDD: autosave at chapter checkpoints).
    /// Put on a trigger collider; the player must be tagged "Player".</summary>
    public class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private bool oneTime = true;
        private bool used;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used && oneTime) return;
            if (!other.CompareTag("Player")) return;
            var gm = GameManager.Instance;
            if (gm == null) return;
            var health = other.GetComponent<Player.PlayerHealth>();
            gm.ReachCheckpoint(other.transform.position, health);
            used = true;
        }
    }
}

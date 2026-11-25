using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SilentWard.Core;
using SilentWard.Dialogue;
using SilentWard.Player;

namespace SilentWard.UI
{
    /// <summary>Chapter HUD (GDD §5): health + focus bars, current objective,
    /// interact prompt, dialogue panel with choice buttons, and the touch
    /// controls — joystick/buttons write into PlayerController's public input
    /// fields, so touch and keyboard share one path. Wire the button methods
    /// to UI Buttons (pointer down/up) in the Chapter01 scene.</summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Bars & objective")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider focusSlider;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI promptText;

        [Header("Dialogue")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueSpeaker;
        [SerializeField] private TextMeshProUGUI dialogueLine;
        [SerializeField] private Transform choicesPanel;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Player")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private DialogueSystem dialogue;

        /// <summary>Chapter controllers assign this to receive the chosen reaction id.</summary>
        public Action<string> OnChoiceSelected;

        private string objectiveKey;

        private void Start()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (health == null) health = FindObjectOfType<PlayerHealth>();
            if (dialogue == null) dialogue = FindObjectOfType<DialogueSystem>();
            if (dialogue != null)
            {
                dialogue.OnLineShown += ShowLine;
                dialogue.OnDialogueEnded += HideDialogue;
            }
            if (health != null)
            {
                health.OnHealthChanged += (v, max) => { if (healthSlider != null) healthSlider.value = v / max; };
                health.OnFocusChanged += (v, max) => { if (focusSlider != null) focusSlider.value = v / max; };
            }
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (promptText != null) promptText.gameObject.SetActive(false);
            var loc = LocalizationManager.Instance;
            if (loc != null) loc.OnLanguageChanged += RefreshObjective;
        }

        private void OnDestroy()
        {
            var loc = LocalizationManager.Instance;
            if (loc != null) loc.OnLanguageChanged -= RefreshObjective;
            if (dialogue != null)
            {
                dialogue.OnLineShown -= ShowLine;
                dialogue.OnDialogueEnded -= HideDialogue;
            }
        }

        private void Update()
        {
            if (promptText == null || player == null) return;
            var target = player.CurrentInteractable;
            bool show = target != null
                && (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing);
            promptText.gameObject.SetActive(show);
            if (show)
            {
                var loc = LocalizationManager.Instance;
                promptText.text = loc != null ? loc.Get(target.InteractionKey) : target.InteractionKey;
            }
        }

        public void SetObjective(string localizationKey)
        {
            objectiveKey = localizationKey;
            RefreshObjective();
        }

        private void RefreshObjective()
        {
            var loc = LocalizationManager.Instance;
            if (objectiveText != null && loc != null && !string.IsNullOrEmpty(objectiveKey))
                objectiveText.text = loc.Get(objectiveKey);
        }

        private void ShowLine(DialogueLine line)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            var loc = LocalizationManager.Instance;
            if (dialogueSpeaker != null)
                dialogueSpeaker.text = loc != null ? loc.Get(line.speakerKey) : line.speakerKey;
            if (dialogueLine != null)
                dialogueLine.text = loc != null ? loc.Get(line.textKey) : line.textKey;
            if (choicesPanel != null)
            {
                foreach (Transform child in choicesPanel) Destroy(child.gameObject);
                foreach (var choice in line.choices)
                {
                    var captured = choice;
                    var btn = Instantiate(choiceButtonPrefab, choicesPanel);
                    var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                        label.text = loc != null ? loc.Get(captured.textKey) : captured.textKey;
                    btn.onClick.AddListener(() =>
                    {
                        string reaction = dialogue?.Choose(captured);
                        OnChoiceSelected?.Invoke(reaction);
                        dialogue?.Next();
                    });
                }
            }
        }

        private void HideDialogue()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        // --- Touch controls: called from UI Buttons (pointer down/up) ---

        public void OnMoveButton(float direction)
        {
            if (player != null) player.MoveInput = direction;
        }

        public void OnRunButton(bool held)
        {
            if (player != null) player.RunHeld = held;
        }

        public void OnCrouchButton(bool held)
        {
            if (player != null) player.CrouchHeld = held;
        }

        public void OnJumpButton()
        {
            player?.QueueJump();
        }

        public void OnInteractButton()
        {
            player?.QueueInteract();
        }

        public void OnDialogueAdvance()
        {
            dialogue?.Next();
        }
    }
}

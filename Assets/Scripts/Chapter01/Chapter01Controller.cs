using System;
using UnityEngine;
using SilentWard.Audio;
using SilentWard.Core;
using SilentWard.Dialogue;
using SilentWard.Player;
using SilentWard.Puzzle;
using SilentWard.UI;

namespace SilentWard.Chapter01
{
    /// <summary>Chapter 1 flow — "بیداری" (GDD §4, ch. 1, ~35 min, tutorial chapter).
    /// Wake up → wristband → admission form (signed in his own hand) → nurse's
    /// note → generator puzzle (2-1-3) → Leila's voice from the speaker →
    /// service elevator opens to a basement that is not on the hospital map.
    /// Wired in the Chapter01 scene; objectives are localization keys shown by the HUD.</summary>
    public class Chapter01Controller : MonoBehaviour
    {
        public enum Stage
        {
            WakeUp, FindWristband, ReadAdmission, RestorePower,
            HearLeila, OpenElevator, Done
        }

        [Header("Scene refs")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private DialogueSystem dialogue;
        [SerializeField] private GeneratorPuzzle generator;
        [SerializeField] private GameObject elevatorDoor; // closed door object; disabled when power returns
        [SerializeField] private GameObject elevatorTrigger; // interactable that opens the elevator

        public Stage CurrentStage { get; private set; } = Stage.WakeUp;
        public event Action<Stage> OnStageChanged;
        public event Action OnChapterComplete;

        private HUDController hud;
        private bool leilaHeard;
        private bool wristbandRead, admissionRead, noteRead;
        private Action afterDialogue;

        private void Start()
        {
            hud = FindObjectOfType<HUDController>();
            if (hud != null) hud.OnChoiceSelected = OnChoiceMade;
            if (dialogue != null) dialogue.OnDialogueEnded += OnDialogueEnded;
            if (generator != null) generator.OnSolved += OnGeneratorSolved;
            if (elevatorTrigger != null) elevatorTrigger.SetActive(false);
            SetStage(Stage.WakeUp);
            Play(Chapter01Dialogue.WakeDialogue(), () => FinishWakeUp());
        }

        private void OnDestroy()
        {
            if (dialogue != null) dialogue.OnDialogueEnded -= OnDialogueEnded;
            if (generator != null) generator.OnSolved -= OnGeneratorSolved;
        }

        private void SetStage(Stage s)
        {
            CurrentStage = s;
            hud?.SetObjective(ObjectiveKey(s));
            OnStageChanged?.Invoke(s);
        }

        private static string ObjectiveKey(Stage s) => s switch
        {
            Stage.WakeUp => "ch1.obj.wake",
            Stage.FindWristband => "ch1.obj.wristband",
            Stage.ReadAdmission => "ch1.obj.admission",
            Stage.RestorePower => "ch1.obj.power",
            Stage.HearLeila => "ch1.obj.leila",
            Stage.OpenElevator => "ch1.obj.elevator",
            _ => "ch1.obj.done",
        };

        private void OnDialogueEnded()
        {
            var next = afterDialogue;
            afterDialogue = null;
            next?.Invoke();
        }

        private void Play(DialogueData data, Action after = null)
        {
            afterDialogue = after;
            dialogue?.Play(data);
        }

        /// <summary>Called by DocumentPickup when the player reads a document.</summary>
        public void NotifyDocumentRead(string documentId)
        {
            AudioManager.Instance?.PlaySfx(ProceduralAudio.Pickup, 0.8f);
            switch (documentId)
            {
                case "wristband":
                    if (wristbandRead) break;
                    wristbandRead = true;
                    Play(Chapter01Dialogue.WristbandDialogue(),
                        () =>
                        {
                            if (CurrentStage != Stage.FindWristband) return;
                            SetStage(admissionRead ? Stage.RestorePower : Stage.ReadAdmission);
                        });
                    break;
                case "admission_form":
                    if (admissionRead) break;
                    admissionRead = true;
                    Play(Chapter01Dialogue.AdmissionDialogue());
                    break;
                case "nurse_note":
                    if (noteRead) break;
                    noteRead = true;
                    Play(Chapter01Dialogue.NurseNoteDialogue(),
                        () => { if (CurrentStage == Stage.ReadAdmission) SetStage(Stage.RestorePower); });
                    break;
            }
        }

        /// <summary>HUD forwards the player's dialogue choice here; the reaction
        /// line plays, then the stage advances. Reactions never branch the story.</summary>
        public void OnChoiceMade(string reactionId)
        {
            if (string.IsNullOrEmpty(reactionId)) return;
            Play(Chapter01Dialogue.ReactionDialogue(reactionId),
                () => { if (CurrentStage == Stage.ReadAdmission) SetStage(Stage.RestorePower); });
        }

        private void OnGeneratorSolved()
        {
            if (CurrentStage == Stage.RestorePower)
                SetStage(leilaHeard ? Stage.OpenElevator : Stage.HearLeila);
            if (elevatorDoor != null) elevatorDoor.SetActive(false);
            if (elevatorTrigger != null) elevatorTrigger.SetActive(true);
            SaveCheckpoint();
        }

        /// <summary>Called once by LeilaTrigger when the player walks past the speaker.</summary>
        public void TriggerLeila()
        {
            if (leilaHeard) return;
            leilaHeard = true;
            AudioManager.Instance?.PlaySfx(ProceduralAudio.SpeakerStatic, 0.7f);
            health?.DrainFocus(15f); // first hallucination: focus, not health
            Play(Chapter01Dialogue.LeilaDialogue(),
                () => { if (CurrentStage == Stage.HearLeila) SetStage(Stage.OpenElevator); });
        }

        /// <summary>Called by the elevator interactable once power is restored.</summary>
        public void OpenElevator()
        {
            if (CurrentStage != Stage.OpenElevator) return;
            AudioManager.Instance?.PlaySfx(ProceduralAudio.ElevatorDing, 0.9f);
            AudioManager.Instance?.PlaySfx(ProceduralAudio.Door, 0.5f);
            Play(Chapter01Dialogue.ElevatorDialogue(), () =>
            {
                SetStage(Stage.Done);
                SaveCheckpoint();
                Play(Chapter01Dialogue.CompleteDialogue(), () => OnChapterComplete?.Invoke());
            });
        }

        public void SaveCheckpoint()
        {
            if (GameManager.Instance == null || player == null) return;
            GameManager.Instance.ReachCheckpoint(player.transform.position, health);
        }

        // Called by the wake-up trigger at the bed, after the opening dialogue.
        public void FinishWakeUp()
        {
            if (CurrentStage == Stage.WakeUp) SetStage(Stage.FindWristband);
        }
    }

    /// <summary>Trigger zone by the ward speaker: first hallucination (Leila's voice).</summary>
    public class LeilaTrigger : MonoBehaviour
    {
        [SerializeField] private Chapter01Controller chapter;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) chapter?.TriggerLeila();
        }
    }

    /// <summary>Service elevator door. Active only after power returns;
    /// opens the way down when the player interacts at stage OpenElevator.</summary>
    public class ElevatorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Chapter01Controller chapter;
        public string InteractionKey => "interact.elevator";
        public bool CanInteract => chapter != null
            && chapter.CurrentStage == Chapter01Controller.Stage.OpenElevator;
        public void Interact() => chapter?.OpenElevator();
    }
}

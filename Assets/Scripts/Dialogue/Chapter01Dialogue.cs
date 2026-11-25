using System.Collections.Generic;
using SilentWard.Dialogue;

namespace SilentWard.Chapter01
{
    /// <summary>All Chapter 1 dialogue content (GDD §4, ch. 1 "بیداری").
    /// Lines are localization keys — Persian and English text lives in
    /// Assets/Resources/Localization/{fa,en}.json. Choices change reactions
    /// only; the main story path never branches (DialogueSystem contract).</summary>
    public static class Chapter01Dialogue
    {
        private static DialogueLine L(string speaker, string text) =>
            new DialogueLine { speakerKey = speaker, textKey = text };

        private static DialogueLine ChoiceLine(string speaker, string text,
            (string textKey, string reactionId) c1, (string textKey, string reactionId) c2)
        {
            var line = L(speaker, text);
            line.choices = new List<DialogueChoice>
            {
                new DialogueChoice { textKey = c1.textKey, reactionId = c1.reactionId },
                new DialogueChoice { textKey = c2.textKey, reactionId = c2.reactionId },
            };
            return line;
        }

        public static DialogueData WakeDialogue() => new DialogueData
        {
            id = "ch1.wake",
            lines = new List<DialogueLine>
            {
                L("speaker.kian", "ch1.wake.1"),
                L("speaker.kian", "ch1.wake.2"),
                L("speaker.kian", "ch1.wake.3"),
            }
        };

        public static DialogueData WristbandDialogue() => new DialogueData
        {
            id = "ch1.wristband",
            lines = new List<DialogueLine>
            {
                L("speaker.kian", "ch1.wristband.1"),
                L("speaker.kian", "ch1.wristband.2"),
            }
        };

        public static DialogueData AdmissionDialogue() => new DialogueData
        {
            id = "ch1.admission",
            lines = new List<DialogueLine>
            {
                L("speaker.kian", "ch1.admission.1"),
                L("speaker.kian", "ch1.admission.2"),
                ChoiceLine("speaker.kian", "ch1.admission.3",
                    ("ch1.admission.choice.deny", "deny"),
                    ("ch1.admission.choice.accept", "accept")),
            }
        };

        public static DialogueData ReactionDialogue(string reactionId) => new DialogueData
        {
            id = "ch1.reaction." + reactionId,
            lines = new List<DialogueLine>
            {
                L("speaker.kian", reactionId == "accept"
                    ? "ch1.admission.reaction.accept"
                    : "ch1.admission.reaction.deny"),
            }
        };

        public static DialogueData NurseNoteDialogue() => new DialogueData
        {
            id = "ch1.note",
            lines = new List<DialogueLine>
            {
                L("speaker.sara", "ch1.note.1"),
                L("speaker.kian", "ch1.note.2"),
            }
        };

        public static DialogueData LeilaDialogue() => new DialogueData
        {
            id = "ch1.leila",
            lines = new List<DialogueLine>
            {
                L("speaker.leila", "ch1.leila.1"),
                L("speaker.kian", "ch1.leila.2"),
            }
        };

        public static DialogueData ElevatorDialogue() => new DialogueData
        {
            id = "ch1.elevator",
            lines = new List<DialogueLine>
            {
                L("speaker.kian", "ch1.elevator.1"),
            }
        };

        public static DialogueData CompleteDialogue() => new DialogueData
        {
            id = "ch1.complete",
            lines = new List<DialogueLine>
            {
                L("speaker.narrator", "ch1.complete.1"),
            }
        };

        /// <summary>Play a Chapter 1 dialogue by id; unknown ids do nothing.</summary>
        public static void PlayDialogue(DialogueSystem system, string id)
        {
            if (system == null) return;
            DialogueData data = id switch
            {
                "wake" => WakeDialogue(),
                "wristband" => WristbandDialogue(),
                "admission" => AdmissionDialogue(),
                "note" => NurseNoteDialogue(),
                "leila" => LeilaDialogue(),
                "elevator" => ElevatorDialogue(),
                "complete" => CompleteDialogue(),
                _ => null,
            };
            if (data != null) system.Play(data);
        }
    }
}

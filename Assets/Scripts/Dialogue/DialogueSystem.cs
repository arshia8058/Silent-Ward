using System;
using System.Collections.Generic;
using UnityEngine;
using SilentWard.Core;

namespace SilentWard.Dialogue
{
    [Serializable] public class DialogueChoice { public string textKey; public string reactionId; }
    [Serializable] public class DialogueLine
    {
        public string speakerKey; public string textKey;
        public List<DialogueChoice> choices = new List<DialogueChoice>(); // choices change reactions only, never the main path
    }
    [Serializable] public class DialogueData { public string id; public List<DialogueLine> lines = new List<DialogueLine>(); }

    /// <summary>Queue-based dialogue player. UI subscribes to events; choices return a reactionId
    /// that NPCs use to pick their response line — the main story path never branches.</summary>
    public class DialogueSystem : MonoBehaviour
    {
        public event Action<DialogueLine> OnLineShown;
        public event Action OnDialogueEnded;
        private Queue<DialogueLine> queue = new();
        public bool IsPlaying { get; private set; }
        public DialogueLine Current { get; private set; }

        public void Play(DialogueData data)
        {
            queue = new Queue<DialogueLine>(data.lines);
            IsPlaying = true;
            GameManager.Instance?.SetState(GameState.Dialogue);
            Next();
        }

        public void Next()
        {
            if (queue.Count == 0) { End(); return; }
            Current = queue.Dequeue();
            OnLineShown?.Invoke(Current);
        }

        /// <summary>UI calls this with the chosen reaction; the NPC response lines are queued by the caller.</summary>
        public string Choose(DialogueChoice choice) => choice != null ? choice.reactionId : null;

        private void End()
        {
            IsPlaying = false; Current = null;
            GameManager.Instance?.SetState(GameState.Playing);
            OnDialogueEnded?.Invoke();
        }
    }
}

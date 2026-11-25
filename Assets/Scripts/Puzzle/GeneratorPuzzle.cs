using System.Collections.Generic;
using UnityEngine;
using SilentWard.Audio;
using SilentWard.Core;

namespace SilentWard.Puzzle
{
    /// <summary>Chapter 1 — emergency generator (GDD §4, ch. 1).
    /// Three switches must be pressed in the order from the nurse's note:
    /// second, first, third (2-1-3). A wrong press resets the sequence and
    /// flickers the lights. Solving restores power: lights on, service
    /// elevator unlocked (Chapter01Controller listens to OnSolved).</summary>
    public class GeneratorPuzzle : PuzzleBase
    {
        [SerializeField] private List<int> correctOrder = new List<int> { 2, 1, 3 };
        [SerializeField] private AudioClip switchClickClip;
        [SerializeField] private AudioClip powerOnClip;

        private readonly List<int> pressed = new List<int>();
        public IReadOnlyList<int> PressedSoFar => pressed;
        public int SwitchCount => correctOrder.Count;

        /// <summary>Called by each switch (SwitchInteractable) or its UI button.</summary>
        public void PressSwitch(int index)
        {
            if (IsSolved) return;
            if (index < 1 || index > correctOrder.Count) return;
            if (switchClickClip != null) AudioManager.Instance?.PlaySfx(switchClickClip);

            pressed.Add(index);
            int step = pressed.Count - 1;
            if (pressed[step] != correctOrder[step])
            {
                pressed.Clear();
                MarkWrongAttempt();
                return;
            }
            if (pressed.Count == correctOrder.Count) TrySolve();
        }

        public override bool TrySolve()
        {
            if (IsSolved) return true;
            if (pressed.Count != correctOrder.Count) return false;
            for (int i = 0; i < correctOrder.Count; i++)
                if (pressed[i] != correctOrder[i]) return false;
            if (powerOnClip != null) AudioManager.Instance?.PlaySfx(powerOnClip);
            MarkSolved();
            return true;
        }

        public override void ResetPuzzle()
        {
            base.ResetPuzzle();
            pressed.Clear();
        }
    }

    /// <summary>One physical switch on the generator panel. Put on a trigger
    /// collider on the interact layer; PlayerController picks it up via IInteractable.</summary>
    public class SwitchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GeneratorPuzzle puzzle;
        [SerializeField] private int switchIndex = 1; // 1-based, matches the nurse's note

        public string InteractionKey => "interact.switch";
        public bool CanInteract => puzzle != null && !puzzle.IsSolved;

        public void Interact()
        {
            if (puzzle == null) return;
            puzzle.PressSwitch(switchIndex);
        }
    }
}

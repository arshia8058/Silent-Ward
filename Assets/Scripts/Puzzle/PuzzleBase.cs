using System;
using UnityEngine;

namespace SilentWard.Puzzle
{
    /// <summary>Base for every environmental puzzle (generator, keypad, tape order...).
    /// Puzzles are solved from clues found in the world — never by random guessing —
    /// and a limited hint is available through the HUD.</summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        public bool IsSolved { get; protected set; }
        public event Action OnSolved;
        public event Action OnWrongAttempt;

        /// <summary>Try to solve; returns true the moment the puzzle is solved.</summary>
        public abstract bool TrySolve();

        public virtual void ResetPuzzle()
        {
            IsSolved = false;
        }

        protected void MarkSolved()
        {
            if (IsSolved) return;
            IsSolved = true;
            OnSolved?.Invoke();
        }

        protected void MarkWrongAttempt() => OnWrongAttempt?.Invoke();
    }
}

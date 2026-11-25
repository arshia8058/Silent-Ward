using UnityEngine;
using UnityEngine.EventSystems;
using SilentWard.Audio;
using SilentWard.Player;

namespace SilentWard.UI
{
    /// <summary>On-screen touch button that drives PlayerController's public
    /// input fields (and the HUD dialogue advance) directly in code, so scenes
    /// need no inspector UnityEvent wiring. Put it on a UI object that has a
    /// raycast-target graphic (e.g. an Image).
    ///
    /// Robustness (controls audit): held state is tracked per action and is
    /// released on pointer-up, pointer-exit AND disable — previously a finger
    /// sliding off a button (or the button being disabled) left the input
    /// stuck on forever. Left/right are resolved from two independent held
    /// flags, so holding both and releasing one no longer kills the other.</summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Action
        {
            MoveLeft = 0,
            MoveRight = 1,
            Run = 2,
            Crouch = 3,
            Jump = 4,
            Interact = 5,
            DialogueAdvance = 6
        }

        [SerializeField] private Action action;

        private static bool leftHeld, rightHeld;
        private bool held;

        private PlayerController player;
        private HUDController hud;

        private void Resolve()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (hud == null) hud = FindObjectOfType<HUDController>();
        }

        private void ApplyMove()
        {
            Resolve();
            if (player == null) return;
            player.MoveInput = (rightHeld ? 1f : 0f) + (leftHeld ? -1f : 0f);
        }

        private void Press(bool down)
        {
            if (held == down) return;
            held = down;
            Resolve();
            switch (action)
            {
                case Action.MoveLeft: leftHeld = down; ApplyMove(); break;
                case Action.MoveRight: rightHeld = down; ApplyMove(); break;
                case Action.Run: if (player != null) player.RunHeld = down; break;
                case Action.Crouch: if (player != null) player.CrouchHeld = down; break;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Resolve();
            AudioManager.Instance?.PlaySfx(ProceduralAudio.Click, 0.5f);
            switch (action)
            {
                case Action.Jump: player?.QueueJump(); break;
                case Action.Interact: player?.QueueInteract(); break;
                case Action.DialogueAdvance: hud?.OnDialogueAdvance(); break;
                default: Press(true); break;
            }
        }

        public void OnPointerUp(PointerEventData eventData) => Press(false);
        public void OnPointerExit(PointerEventData eventData) => Press(false);
        private void OnDisable() => Press(false);
    }
}

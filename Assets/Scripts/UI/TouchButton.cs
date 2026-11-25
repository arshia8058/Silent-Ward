using UnityEngine;
using UnityEngine.EventSystems;
using SilentWard.Audio;
using SilentWard.Player;

namespace SilentWard.UI
{
    /// <summary>On-screen touch button that drives PlayerController's public
    /// input fields (and the HUD dialogue advance) directly in code, so scenes
    /// need no inspector UnityEvent wiring. Put it on a UI object that has a
    /// raycast-target graphic (e.g. an Image).</summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

        private PlayerController player;
        private HUDController hud;

        private void Resolve()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (hud == null) hud = FindObjectOfType<HUDController>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Resolve();
            AudioManager.Instance?.PlaySfx(ProceduralAudio.Click, 0.5f);
            switch (action)
            {
                case Action.MoveLeft: if (player != null) player.MoveInput = -1f; break;
                case Action.MoveRight: if (player != null) player.MoveInput = 1f; break;
                case Action.Run: if (player != null) player.RunHeld = true; break;
                case Action.Crouch: if (player != null) player.CrouchHeld = true; break;
                case Action.Jump: player?.QueueJump(); break;
                case Action.Interact: player?.QueueInteract(); break;
                case Action.DialogueAdvance: hud?.OnDialogueAdvance(); break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Resolve();
            switch (action)
            {
                case Action.MoveLeft: if (player != null && player.MoveInput < 0f) player.MoveInput = 0f; break;
                case Action.MoveRight: if (player != null && player.MoveInput > 0f) player.MoveInput = 0f; break;
                case Action.Run: if (player != null) player.RunHeld = false; break;
                case Action.Crouch: if (player != null) player.CrouchHeld = false; break;
            }
        }
    }
}

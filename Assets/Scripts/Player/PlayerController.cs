using UnityEngine;
using SilentWard.Core;

namespace SilentWard.Player
{
    /// <summary>
    /// 2D movement: walk, run (stamina), jump, crouch (narrow passages + stealth), interact.
    /// Input comes from TouchControls (joystick/buttons) or keyboard for editor testing;
    /// both write into the same fields so gameplay code never cares about the source.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float runSpeed = 5.4f;
        [SerializeField] private float jumpForce = 8.5f;
        [SerializeField] private float crouchSpeedFactor = 0.45f;
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float runDrain = 22f;   // per second
        [SerializeField] private float staminaRegen = 14f;
        [Header("Ground / Interact")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float interactRadius = 0.9f;
        [SerializeField] private LayerMask interactLayer;

        // --- input state (written by touch UI or keyboard) ---
        public float MoveInput; public bool RunHeld; public bool CrouchHeld;
        private bool jumpQueued, interactQueued;

        private Rigidbody2D rb; private CapsuleCollider2D col;
        private Vector2 standingSize, standingOffset;
        public float Stamina { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsMoving => Mathf.Abs(MoveInput) > 0.05f && IsGrounded;
        public int Facing { get; private set; } = 1;
        public IInteractable CurrentInteractable { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>(); col = GetComponent<CapsuleCollider2D>();
            standingSize = col.size; standingOffset = col.offset; Stamina = maxStamina;
        }

        public void QueueJump() => jumpQueued = true;
        public void QueueInteract() => interactQueued = true;

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing)
            {
                MoveInput = Input.GetAxisRaw("Horizontal");
                RunHeld = Input.GetKey(KeyCode.LeftShift);
                CrouchHeld = Input.GetKey(KeyCode.LeftControl);
                if (Input.GetKeyDown(KeyCode.Space)) QueueJump();
                if (Input.GetKeyDown(KeyCode.E)) QueueInteract();
            }
#endif
            DetectInteractable();
            if (interactQueued) { interactQueued = false; CurrentInteractable?.Interact(); }
        }

        private void FixedUpdate()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.12f, groundLayer);
            bool wantsRun = RunHeld && IsMoving && !IsCrouching && Stamina > 1f;
            float speed = wantsRun ? runSpeed : walkSpeed;
            IsCrouching = CrouchHeld && IsGrounded;
            if (IsCrouching) speed *= crouchSpeedFactor;

            Stamina = wantsRun ? Mathf.Max(0, Stamina - runDrain * Time.fixedDeltaTime)
                               : Mathf.Min(maxStamina, Stamina + staminaRegen * Time.fixedDeltaTime);

            rb.velocity = new Vector2(MoveInput * speed, rb.velocity.y);
            if (Mathf.Abs(MoveInput) > 0.05f) Facing = MoveInput > 0 ? 1 : -1;

            // crouch: shrink collider so the player fits low passages
            col.size = IsCrouching ? new Vector2(standingSize.x, standingSize.y * 0.55f) : standingSize;
            col.offset = IsCrouching ? new Vector2(standingOffset.x, standingOffset.y - standingSize.y * 0.22f) : standingOffset;

            if (jumpQueued)
            {
                jumpQueued = false;
                if (IsGrounded && !IsCrouching) rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        private void DetectInteractable()
        {
            var hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactLayer);
            CurrentInteractable = hit != null ? hit.GetComponent<IInteractable>() : null;
            if (CurrentInteractable != null && !CurrentInteractable.CanInteract) CurrentInteractable = null;
        }
    }
}

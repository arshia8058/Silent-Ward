using UnityEngine;
using SilentWard.Player;

namespace SilentWard.Art
{
    /// <summary>
    /// Drives Kian's sprite animation from PlayerController state.
    /// Sheets are assigned by ArtBootstrap at runtime; every state is
    /// null-guarded so a missing sheet can never break the game.
    /// </summary>
    public class KianAnimator : MonoBehaviour
    {
        public Sprite[] idle;
        public Sprite[] walk;
        public Sprite[] run;
        public Sprite[] jump;
        public Sprite[] crouch;
        public Sprite[] hurt;
        public Sprite[] death;

        private SpriteRenderer sr;
        private PlayerController pc;
        private Rigidbody2D rb;
        private Sprite[] cur;
        private float curFps = 6f;
        private float timer;
        private int frame;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            pc = GetComponentInParent<PlayerController>();
            rb = GetComponentInParent<Rigidbody2D>();
        }

        private void Update()
        {
            if (sr == null || pc == null) return;
            sr.flipX = pc.Facing < 0;

            if (!pc.IsGrounded && jump != null && jump.Length >= 3)
            {
                float vy = rb != null ? rb.velocity.y : 0f;
                int f = vy > 1f ? 0 : (vy < -1f ? 2 : 1);
                sr.sprite = jump[f];
                cur = null;
                return;
            }

            Sprite[] sheet = idle;
            float fps = 6f;
            if (pc.IsCrouching && crouch != null && crouch.Length > 0) { sheet = crouch; fps = 5f; }
            else if (pc.IsMoving)
            {
                if (pc.RunHeld && run != null && run.Length > 0) { sheet = run; fps = 13f; }
                else if (walk != null && walk.Length > 0) { sheet = walk; fps = 10f; }
            }
            if (sheet == null || sheet.Length == 0) return;

            if (sheet != cur) { cur = sheet; curFps = fps; frame = 0; timer = 0f; }
            timer += Time.deltaTime;
            float dur = 1f / Mathf.Max(1f, curFps);
            if (timer >= dur)
            {
                timer = 0f;
                frame = (frame + 1) % cur.Length;
            }
            sr.sprite = cur[Mathf.Min(frame, cur.Length - 1)];
        }
    }

    /// <summary>Simple looping sheet player for NPC figures (e.g. Leila).</summary>
    public class IdleAnimator : MonoBehaviour
    {
        public Sprite[] frames;
        public float fps = 6f;
        private SpriteRenderer sr;
        private float timer;
        private int frame;

        private void Start() { sr = GetComponent<SpriteRenderer>(); }

        private void Update()
        {
            if (sr == null || frames == null || frames.Length == 0) return;
            timer += Time.deltaTime;
            if (timer >= 1f / Mathf.Max(1f, fps))
            {
                timer = 0f;
                frame = (frame + 1) % frames.Length;
                sr.sprite = frames[frame];
            }
        }
    }
}

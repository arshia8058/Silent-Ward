using UnityEngine;

namespace SilentWard.Audio
{
    /// <summary>Footsteps: while the body moves horizontally and stays
    /// grounded, plays the procedural step clip at a speed-scaled interval.
    /// Added to the player at runtime by AudioBootstrap.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class StepSounds : MonoBehaviour
    {
        private Rigidbody2D rb;
        private float timer;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (rb == null || AudioManager.Instance == null) return;
            float speed = Mathf.Abs(rb.velocity.x);
            bool grounded = Mathf.Abs(rb.velocity.y) < 0.15f;
            if (!grounded || speed < 0.6f) { timer = 0.1f; return; }
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                AudioManager.Instance.PlaySfx(ProceduralAudio.Step, 0.45f);
                timer = Mathf.Clamp(2.4f / speed, 0.27f, 0.48f);
            }
        }
    }
}

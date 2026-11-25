using UnityEngine;

namespace SilentWard.Player
{
    /// <summary>
    /// Safe-spawn guarantee for the player (root-cause fix for spawn falls):
    /// 1) The Rigidbody2D starts non-simulated, so the player cannot fall
    ///    before the world (floor colliders) is confirmed ready.
    /// 2) It is released only when ground is actually detected below the
    ///    spawn point (with a hard time fallback so control is never locked).
    /// 3) If the player ever leaves the world (y &lt; -6), it is returned to
    ///    the spawn point with zero velocity — an infinite fall is impossible.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpawnGuard : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector3 spawnPoint;

        private Rigidbody2D rb;
        private bool released;
        private float waitTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spawnPoint == Vector3.zero) spawnPoint = transform.position;
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        private void FixedUpdate()
        {
            if (released) return;
            waitTime += Time.fixedDeltaTime;
            Vector2 probe = new Vector2(spawnPoint.x, spawnPoint.y - 0.9f);
            bool groundBelow = Physics2D.OverlapCircle(probe, 0.25f, groundLayer);
            if (groundBelow || waitTime > 1.5f)
            {
                rb.simulated = true;
                rb.velocity = Vector2.zero;
                released = true;
            }
        }

        private void Update()
        {
            if (transform.position.y < -6f)
            {
                transform.position = spawnPoint;
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.simulated = true;
                }
                released = true;
            }
        }
    }
}

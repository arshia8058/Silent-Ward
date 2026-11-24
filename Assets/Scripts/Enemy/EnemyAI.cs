using UnityEngine;

namespace SilentWard.Enemy
{
    public enum EnemyState { Patrol, Alert, Chase, Search, Stunned }

    /// <summary>Mid-level enemy AI state machine shared by human guards:
    /// patrol waypoints -> alert on sight/noise -> chase -> search last seen position -> back to patrol.
    /// Shadow-type enemies use their own light-sensitive behaviour instead of this class.</summary>
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float patrolSpeed = 1.6f, chaseSpeed = 3.4f;
        [SerializeField] private float sightRange = 6f, sightAngleDot = 0.35f;
        [SerializeField] private float loseSightAfter = 4f, attackRange = 0.8f, attackDamage = 12f;
        [SerializeField] private LayerMask playerLayer;

        public EnemyState State { get; private set; } = EnemyState.Patrol;
        private int waypointIndex; private Transform player;
        private float searchTimer; private Vector2 lastSeen;

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        private void Update()
        {
            if (player == null) return;
            bool sees = CanSeePlayer();
            switch (State)
            {
                case EnemyState.Patrol:
                    Patrol();
                    if (sees) SetState(EnemyState.Alert);
                    break;
                case EnemyState.Alert: // brief reaction beat, then chase — readable for the player
                    searchTimer += Time.deltaTime;
                    if (sees) SetState(EnemyState.Chase);
                    else if (searchTimer > 0.6f) SetState(EnemyState.Search);
                    break;
                case EnemyState.Chase:
                    lastSeen = player.position;
                    MoveToward(player.position, chaseSpeed);
                    if (Vector2.Distance(transform.position, player.position) <= attackRange) Attack();
                    if (!sees) { searchTimer += Time.deltaTime; if (searchTimer > loseSightAfter) SetState(EnemyState.Search); }
                    else searchTimer = 0;
                    break;
                case EnemyState.Search:
                    MoveToward(lastSeen, patrolSpeed);
                    searchTimer += Time.deltaTime;
                    if (Vector2.Distance(transform.position, lastSeen) < 0.3f && searchTimer > 2.5f) SetState(EnemyState.Patrol);
                    if (sees) SetState(EnemyState.Chase);
                    break;
            }
        }

        private bool CanSeePlayer()
        {
            Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
            if (toPlayer.magnitude > sightRange) return false;
            float facing = transform.localScale.x >= 0 ? 1 : -1;
            // crouching player is harder to spot: shorter effective range (handled via distance check)
            var pc = player.GetComponent<Player.PlayerController>();
            float range = (pc != null && pc.IsCrouching) ? sightRange * 0.55f : sightRange;
            if (toPlayer.magnitude > range) return false;
            return Vector2.Dot(toPlayer.normalized, new Vector2(facing, 0)) > sightAngleDot;
        }

        private void Patrol()
        {
            if (waypoints == null || waypoints.Length == 0) return;
            Vector2 wp = waypoints[waypointIndex].position;
            MoveToward(wp, patrolSpeed);
            if (Vector2.Distance(transform.position, wp) < 0.25f)
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }

        private void MoveToward(Vector2 target, float speed)
        {
            Vector2 pos = transform.position;
            float dir = Mathf.Sign(target.x - pos.x);
            transform.position = new Vector3(pos.x + dir * speed * Time.deltaTime, pos.y, transform.position.z);
            var s = transform.localScale; s.x = Mathf.Abs(s.x) * dir; transform.localScale = s;
        }

        private float attackCooldown;
        private void Attack()
        {
            attackCooldown -= Time.deltaTime; if (attackCooldown > 0) return;
            attackCooldown = 0.9f;
            var health = player.GetComponent<Player.PlayerHealth>();
            if (health != null)
                health.TakeDamage(attackDamage * (Core.GameManager.Instance != null ? Core.GameManager.Instance.EnemyDamageMultiplier : 1f));
        }

        private void SetState(EnemyState s) { State = s; searchTimer = 0; }
        /// <summary>Noise events (running, gunshots) can pull the enemy to a position.</summary>
        public void HearNoise(Vector2 at) { lastSeen = at; if (State == EnemyState.Patrol) SetState(EnemyState.Search); }
    }
}

using UnityEngine;

namespace SilentWard.Camera
{
    /// <summary>Smooth follow camera with look-ahead; cinematic mode overrides the target
    /// for cutscenes and boss fights so story moments get their own framing.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.18f;
        [SerializeField] private float lookAhead = 1.2f;
        [SerializeField] private Vector2 minBounds, maxBounds;
        [SerializeField] private bool useBounds;

        private Vector3 velocity;
        private Transform cinematicTarget; private float cinematicZoom = -1f;
        private UnityEngine.Camera cam;

        private void Awake() => cam = GetComponent<UnityEngine.Camera>();

        public void SetCinematic(Transform t, float orthographicSize) { cinematicTarget = t; cinematicZoom = orthographicSize; }
        public void ClearCinematic() { cinematicTarget = null; cinematicZoom = -1f; }

        private void LateUpdate()
        {
            Transform t = cinematicTarget != null ? cinematicTarget : target;
            if (t == null) return;
            float ahead = 0f;
            var pc = t.GetComponent<Player.PlayerController>();
            if (cinematicTarget == null && pc != null) ahead = pc.Facing * lookAhead;
            Vector3 goal = new Vector3(t.position.x + ahead, t.position.y + 0.6f, transform.position.z);
            if (useBounds && cinematicTarget == null)
            {
                goal.x = Mathf.Clamp(goal.x, minBounds.x, maxBounds.x);
                goal.y = Mathf.Clamp(goal.y, minBounds.y, maxBounds.y);
            }
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
            if (cinematicZoom > 0 && cam != null)
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, cinematicZoom, Time.deltaTime * 2f);
        }
    }
}

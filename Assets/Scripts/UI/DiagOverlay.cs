using UnityEngine;
using UnityEngine.SceneManagement;
using SilentWard.Player;

namespace SilentWard.UI
{
    /// <summary>
    /// TEMPORARY on-device diagnostic overlay (added for the Build #9 debug
    /// round at the project owner's request for root-cause evidence).
    /// Draws with Unity's built-in IMGUI so it renders even if TMP fonts,
    /// the Canvas or the EventSystem fail. One screenshot from the device
    /// shows exactly which subsystem is broken: scene, spawn/physics state,
    /// art placement, Persian font status, and the last runtime error.
    /// Remove this script once the device verdict is clean.
    /// </summary>
    public class DiagOverlay : MonoBehaviour
    {
        private static string lastError = "-";
        private PlayerController player;
        private Rigidbody2D playerRb;
        private float findTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("DiagOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<DiagOverlay>();
            Application.logMessageReceived += OnLog;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) return;
            string first = condition ?? "";
            int nl = first.IndexOf('\n');
            if (nl >= 0) first = first.Substring(0, nl);
            if (first.Length > 100) first = first.Substring(0, 100);
            lastError = first;
        }

        private void Update()
        {
            findTimer -= Time.deltaTime;
            if (findTimer > 0f) return;
            findTimer = 0.5f;
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
                if (player != null) playerRb = player.GetComponent<Rigidbody2D>();
            }
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            GUI.Box(new Rect(8, 8, 560, 168), "");
            float y = 12f;
            Line(style, ref y, "scene: " + SceneManager.GetActiveScene().name);
            if (player != null)
            {
                Line(style, ref y, "player y: " + player.transform.position.y.ToString("F2")
                    + "  grounded: " + player.IsGrounded
                    + "  rb: " + (playerRb != null && playerRb.simulated ? "on" : "off"));
            }
            else Line(style, ref y, "player: not in scene");
            Line(style, ref y, "backdrop: " + (GameObject.Find("ArtBackdrop") != null ? "OK" : "MISSING")
                + "  kian: " + (GameObject.Find("KianSprite") != null ? "OK" : "-"));
            Line(style, ref y, "font-fa: " + (PersianUiBootstrap.FontReady ? "OK" : "searching"));
            Line(style, ref y, "err: " + lastError);
        }

        private static void Line(GUIStyle style, ref float y, string text)
        {
            GUI.Label(new Rect(16, y, 544, 30), text, style);
            y += 31f;
        }
    }
}

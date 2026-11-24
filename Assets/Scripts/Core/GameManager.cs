using System;
using UnityEngine;
using SilentWard.Player;

namespace SilentWard.Core
{
    public enum GameState { MainMenu, Playing, Paused, Dialogue, Cutscene, Dead }
    public enum Difficulty { Easy, Normal, Hard }

    /// <summary>Central game state: chapter flow, difficulty, checkpoints (autosave).</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.MainMenu;
        public Difficulty Difficulty { get; set; } = Difficulty.Normal;
        public int CurrentChapter { get; private set; } = 1;
        public SaveData CurrentData { get; private set; } = new SaveData();

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnChapterChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState s)
        {
            State = s;
            Time.timeScale = (s == GameState.Paused) ? 0f : 1f;
            OnStateChanged?.Invoke(s);
        }

        public void StartChapter(int chapter, string sceneName)
        {
            CurrentChapter = chapter;
            CurrentData.chapter = chapter;
            CurrentData.sceneName = sceneName;
            OnChapterChanged?.Invoke(chapter);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            SetState(GameState.Playing);
        }

        /// <summary>Called by checkpoint triggers in each chapter scene.</summary>
        public void ReachCheckpoint(Vector2 playerPos, PlayerHealth health)
        {
            CurrentData.playerX = playerPos.x; CurrentData.playerY = playerPos.y;
            if (health != null) { CurrentData.health = health.Health; CurrentData.focus = health.Focus; }
            CurrentData.difficulty = (int)Difficulty;
            SaveSystem.Save(0, CurrentData); // autosave slot
        }

        public void ManualSave(int slot, Vector2 playerPos, PlayerHealth health)
        {
            ReachCheckpointData(playerPos, health);
            SaveSystem.Save(slot, CurrentData);
        }

        private void ReachCheckpointData(Vector2 p, PlayerHealth h)
        {
            CurrentData.playerX = p.x; CurrentData.playerY = p.y;
            if (h != null) { CurrentData.health = h.Health; CurrentData.focus = h.Focus; }
        }

        public bool ContinueGame()
        {
            SaveData data = SaveSystem.Load(0);
            if (data == null) return false;
            CurrentData = data;
            Difficulty = (Difficulty)data.difficulty;
            StartChapter(data.chapter, data.sceneName);
            return true;
        }

        // Difficulty tuning used by combat/enemy systems.
        public float EnemyDamageMultiplier => Difficulty switch { Difficulty.Easy => 0.6f, Difficulty.Hard => 1.5f, _ => 1f };
        public float ResourceMultiplier => Difficulty switch { Difficulty.Easy => 1.4f, Difficulty.Hard => 0.7f, _ => 1f };
    }
}

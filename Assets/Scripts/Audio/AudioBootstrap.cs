using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SilentWard.Player;
using SilentWard.Puzzle;

namespace SilentWard.Audio
{
    /// <summary>Runtime audio wiring: starts the procedural music/ambience
    /// through AudioManager and injects the generated SFX clips into the
    /// systems that play them (generator puzzle, footsteps). Created
    /// automatically at startup — no scene wiring needed.</summary>
    public class AudioBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("AudioBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<AudioBootstrap>();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private IEnumerator Start()
        {
            while (AudioManager.Instance == null) yield return null;
            AudioManager.Instance.PlayMusic(ProceduralAudio.Music);
            AudioManager.Instance.PlayAmbient(ProceduralAudio.Ambient);
            WireScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => WireScene();

        private static void WireScene()
        {
            var puzzle = FindObjectOfType<GeneratorPuzzle>();
            if (puzzle != null)
                puzzle.SetClips(ProceduralAudio.SwitchClick, ProceduralAudio.PowerOn);

            var player = FindObjectOfType<PlayerController>();
            if (player != null && player.GetComponent<StepSounds>() == null)
                player.gameObject.AddComponent<StepSounds>();
        }
    }
}

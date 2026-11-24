using System.Collections;
using UnityEngine;

namespace SilentWard.Audio
{
    /// <summary>Music / SFX / ambient channels with independent volumes (from Settings).
    /// Tension music and silence are the main horror tools: CrossfadeTo supports fading to near-silence.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [SerializeField] private AudioSource musicSource, ambientSource;
        [SerializeField] private AudioSource sfxPrefab;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }

        public void SetVolumes(float music, float sfx, float ambient)
        {
            if (musicSource) musicSource.volume = music;
            if (ambientSource) ambientSource.volume = ambient;
            if (sfxPrefab) sfxPrefab.volume = sfx;
        }

        public void PlayMusic(AudioClip clip, float fadeSeconds = 1.5f)
        {
            if (musicSource == null || clip == null) return;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(Crossfade(clip, fadeSeconds));
        }

        private IEnumerator Crossfade(AudioClip clip, float seconds)
        {
            float startVol = musicSource.volume; float t = 0;
            while (t < seconds / 2f) { t += Time.deltaTime; musicSource.volume = Mathf.Lerp(startVol, 0, t / (seconds / 2f)); yield return null; }
            musicSource.clip = clip; musicSource.loop = true; musicSource.Play();
            t = 0;
            while (t < seconds / 2f) { t += Time.deltaTime; musicSource.volume = Mathf.Lerp(0, startVol, t / (seconds / 2f)); yield return null; }
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (ambientSource == null || clip == null) return;
            ambientSource.clip = clip; ambientSource.loop = true; ambientSource.Play();
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxPrefab == null || clip == null) return;
            sfxPrefab.PlayOneShot(clip, volumeScale);
        }
    }
}

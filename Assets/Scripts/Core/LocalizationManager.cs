using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilentWard.Core
{
    [Serializable] public class LocEntry { public string key; public string value; }
    [Serializable] public class LocTable { public List<LocEntry> entries = new List<LocEntry>(); }

    /// <summary>FA/EN localization loaded from Assets/Localization/{fa,en}.json. Switchable at runtime.</summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        public string Language { get; private set; } = "fa";
        public bool IsRTL => Language == "fa";
        public event Action OnLanguageChanged;

        private readonly Dictionary<string, Dictionary<string, string>> tables = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            foreach (string lang in new[] { "fa", "en" })
            {
                TextAsset asset = Resources.Load<TextAsset>($"Localization/{lang}");
                if (asset == null) { Debug.LogWarning($"[Loc] Missing table: {lang}"); continue; }
                var table = JsonUtility.FromJson<LocTable>(asset.text);
                var dict = new Dictionary<string, string>();
                foreach (var e in table.entries) dict[e.key] = e.value;
                tables[lang] = dict;
            }
        }

        public void SetLanguage(string lang)
        {
            if (!tables.ContainsKey(lang)) return;
            Language = lang;
            OnLanguageChanged?.Invoke();
        }

        public string Get(string key)
        {
            if (tables.TryGetValue(Language, out var d) && d.TryGetValue(key, out var v)) return v;
            if (tables.TryGetValue("en", out var en) && en.TryGetValue(key, out var ev)) return ev;
            return key; // visible fallback so missing keys are found in QA, never a crash
        }
    }
}

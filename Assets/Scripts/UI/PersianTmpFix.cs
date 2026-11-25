using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilentWard.UI
{
    /// <summary>Attaches to a TextMeshProUGUI and rewrites Persian text into
    /// shaped visual order (see PersianText) whenever the text changes.
    /// Idempotent: it remembers the last source and its fixed form.</summary>
    public class PersianTmpFixer : MonoBehaviour
    {
        private TextMeshProUGUI tmp;
        private string lastSource, lastFixed;

        private void Awake() => tmp = GetComponent<TextMeshProUGUI>();

        private void LateUpdate()
        {
            if (tmp == null) return;
            string t = tmp.text;
            if (string.IsNullOrEmpty(t) || t == lastFixed) return;
            if (t == lastSource) { tmp.text = lastFixed; return; }
            if (!PersianText.ContainsArabic(t) || t.IndexOf('<') >= 0)
            {
                lastSource = t;
                lastFixed = t;
                return;
            }
            lastSource = t;
            lastFixed = PersianText.Fix(t);
            tmp.text = lastFixed;
        }
    }

    /// <summary>Makes Persian render at all: the bundled LiberationSans SDF
    /// font has no Arabic/Persian glyphs (Persian showed as empty boxes).
    /// At startup this builds a dynamic TMP font from the phone's own OS
    /// Arabic font (Noto family on Android) and registers it as a fallback
    /// on every loaded TMP font asset, then attaches PersianTmpFixer to
    /// every TMP text, including ones created later (choice buttons).</summary>
    public class PersianUiBootstrap : MonoBehaviour
    {
        private static bool fontReady;
        public static bool FontReady => fontReady;
        private float scanTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("PersianUiBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<PersianUiBootstrap>();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnScene;
        private void OnDisable() => SceneManager.sceneLoaded -= OnScene;

        private void Start()
        {
            SetupFont();
            AttachAll();
        }

        private void OnScene(Scene scene, LoadSceneMode mode)
        {
            SetupFont();
            AttachAll();
        }

        private void Update()
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f)
            {
                scanTimer = 0.5f;
                if (!fontReady) SetupFont();
                AttachAll();
            }
        }

        private static void AttachAll()
        {
            foreach (var t in FindObjectsOfType<TextMeshProUGUI>())
                if (t.GetComponent<PersianTmpFixer>() == null)
                    t.gameObject.AddComponent<PersianTmpFixer>();
        }

        private static void SetupFont()
        {
            if (fontReady) return;
            try
            {
                var osFont = FindPersianFont();
                if (osFont == null) return; // retry on next scene load / scan

                var dyn = TMP_FontAsset.CreateFontAsset(osFont);
                if (dyn == null) return;
                dyn.name = "PersianOSFont";
                try
                {
                    string missing;
                    dyn.TryAddCharacters(
                        "ابپتثجچحخدذرزژسشصضطظعغفقکگلمنوهیآأإؤئءةۀ۱۲۳۴۵۶۷۸۹۰0123456789ـ،؛؟!«»:؛", out missing);
                }
                catch (System.Exception) { }

                foreach (var fa in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                {
                    if (fa == null || fa == dyn) continue;
                    var table = fa.fallbackFontAssetTable;
                    if (table != null && !table.Contains(dyn)) table.Add(dyn);
                }
                fontReady = true;
            }
            catch (System.Exception)
            {
                // Never crash over a font — worst case text stays as boxes.
            }
        }

        /// <summary>Finds a font on THIS device that really contains Persian
        /// glyphs. The old version trusted a few hard-coded names; on phones
        /// where those names miss (e.g. MIUI), it silently fell back to a
        /// Latin-only font and Persian stayed as boxes. Now every installed
        /// font is a candidate and each is verified with HasCharacter.</summary>
        private static Font FindPersianFont()
        {
            string[] preferred =
            {
                "Noto Naskh Arabic", "Noto Sans Arabic", "Noto Naskh Arabic UI",
                "Noto Sans Arabic UI", "Droid Arabic Naskh", "Droid Sans Arabic",
                "Vazirmatn", "Vazir", "Tahoma", "Arial"
            };
            foreach (var name in preferred)
            {
                var f = TryFont(name);
                if (f != null) return f;
            }
            try
            {
                foreach (var name in Font.GetOSInstalledFontNames())
                {
                    if (string.IsNullOrEmpty(name)) continue;
                    var f = TryFont(name);
                    if (f != null) return f;
                }
            }
            catch (System.Exception) { }
            return null;
        }

        private static Font TryFont(string name)
        {
            try
            {
                var f = Font.CreateDynamicFontFromOSFont(name, 36);
                if (f == null) return null;
                f.RequestCharactersInTexture("سلام", 36, FontStyle.Normal);
                bool ok = f.HasCharacter('س') && f.HasCharacter('ا') && f.HasCharacter('م');
                if (!ok) { Destroy(f); return null; }
                return f;
            }
            catch (System.Exception) { return null; }
        }
    }
}

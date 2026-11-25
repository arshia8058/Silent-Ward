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
            fontReady = true;
            try
            {
                var osFont = Font.CreateDynamicFontFromOSFont(
                    new[]
                    {
                        "Noto Naskh Arabic", "Noto Sans Arabic",
                        "Droid Arabic Naskh", "Droid Sans", "sans-serif"
                    }, 36);
                if (osFont == null) return;

                var dyn = TMP_FontAsset.CreateFontAsset(osFont);
                if (dyn == null) return;
                dyn.name = "PersianOSFont";

                foreach (var fa in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                {
                    if (fa == null || fa == dyn) continue;
                    var table = fa.fallbackFontAssetTable;
                    if (table != null && !table.Contains(dyn)) table.Add(dyn);
                }
            }
            catch (System.Exception)
            {
                // Never crash over a font — worst case text stays as boxes.
            }
        }
    }
}

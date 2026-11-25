#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Generates the app icon during the build: a dark doorway with a
/// warm slit of light (Silent Ward motif). The project ships no binary
/// textures (the GitHub push path is text-only), so the icon is drawn
/// procedurally here, on the build machine, and assigned to the Android
/// target before packaging. Any failure is swallowed — a default icon is
/// better than a failed build.</summary>
public class IconBuilder : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;

    public void OnPreprocessBuild(BuildReport report)
    {
        try { Build(); }
        catch (System.Exception) { /* never fail a build over an icon */ }
    }

    private static Color32 C(int r, int g, int b) =>
        new Color32((byte)r, (byte)g, (byte)b, 255);

    private static Color32 Blend(Color32 baseC, Color32 over, float a)
    {
        return new Color32(
            (byte)Mathf.Lerp(baseC.r, over.r, a),
            (byte)Mathf.Lerp(baseC.g, over.g, a),
            (byte)Mathf.Lerp(baseC.b, over.b, a), 255);
    }

    private static void Build()
    {
        const int S = 512;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px = new Color32[S * S];

        var bgTop = C(10, 12, 24);
        var bgBottom = C(3, 4, 8);
        var frame = C(24, 27, 40);
        var door = C(10, 12, 19);
        var light = C(255, 201, 120);

        for (int y = 0; y < S; y++)
        {
            float v = y / (float)(S - 1);
            var bg = new Color32(
                (byte)Mathf.Lerp(bgBottom.r, bgTop.r, v),
                (byte)Mathf.Lerp(bgBottom.g, bgTop.g, v),
                (byte)Mathf.Lerp(bgBottom.b, bgTop.b, v), 255);
            for (int x = 0; x < S; x++)
            {
                var c = bg;
                // Door frame and door leaf (centered).
                if (x >= 176 && x < 336 && y >= 42 && y < 470) c = frame;
                if (x >= 190 && x < 322 && y >= 56 && y < 456) c = door;
                // The door is ajar: a bright slit on its right side.
                if (x >= 300 && x < 322 && y >= 56 && y < 456)
                {
                    float edge = 1f - Mathf.Abs(x - 311) / 11f;
                    float vert = 0.75f + 0.25f * Mathf.Sin(Mathf.PI * (y - 56) / 400f);
                    c = Blend(c, light, Mathf.Clamp01(edge * vert + 0.25f));
                }
                // Light spilling out to the right, fading with distance.
                if (x >= 322 && x < 430 && y >= 56 && y < 456)
                {
                    float a = (1f - (x - 322) / 108f) * 0.38f
                              * (0.6f + 0.4f * Mathf.Sin(Mathf.PI * (y - 56) / 400f));
                    c = Blend(c, light, Mathf.Clamp01(a));
                }
                // A faint pool of light on the floor below the door.
                if (y < 56 && x >= 220 && x < 430)
                {
                    float a = (y / 56f) * (1f - Mathf.Abs(x - 325) / 210f) * 0.30f;
                    c = Blend(c, light, Mathf.Clamp01(a));
                }
                px[y * S + x] = c;
            }
        }

        tex.SetPixels32(px);
        tex.Apply();

        const string dir = "Assets/Art/Generated";
        Directory.CreateDirectory(dir);
        string path = dir + "/app-icon-512.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (icon == null) return;

        var sizes = PlayerSettings.GetIconSizesForTargetGroup(
            BuildTargetGroup.Android, IconKind.Application);
        if (sizes == null || sizes.Length == 0) return;
        var icons = new Texture2D[sizes.Length];
        for (int i = 0; i < sizes.Length; i++) icons[i] = icon;
        PlayerSettings.SetIconsForTargetGroup(
            BuildTargetGroup.Android, icons, IconKind.Application);
    }
}
#endif

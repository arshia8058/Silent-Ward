using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SilentWard.Art
{
    /// <summary>
    /// Runtime art integration for Silent Ward. The game's artwork ships as
    /// Base64 text (ArtData1..6) because the delivery pipeline is text-only;
    /// this bootstrap decodes it at runtime and dresses the scenes:
    /// menu backdrop, Chapter 1 backdrop and props, pickup visuals, Leila's
    /// figure, and Kian's animated sprite (via KianAnimator).
    /// Everything is defensive: if any asset fails, the placeholder visuals
    /// stay and the game keeps working.
    /// </summary>
    public static class ArtBootstrap
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        private static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!hooked)
            {
                hooked = true;
                SceneManager.sceneLoaded += OnScene;
            }
            Apply(SceneManager.GetActiveScene());
        }

        private static void OnScene(Scene scene, LoadSceneMode mode) { Apply(scene); }

        private static void Apply(Scene scene)
        {
            try
            {
                FixCamera();
                if (scene.name == "MainMenu") ApplyMenu();
                else if (scene.name == "Chapter01") ApplyChapter();
            }
            catch (Exception e) { Debug.Log("ArtBootstrap apply failed: " + e.Message); }
        }

        /// <summary>Runtime guarantee: the scene camera always paints a solid
        /// dark background, whatever the scene file says.</summary>
        private static void FixCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f, 1f);
        }

        private static Sprite SolidSprite(Color c)
        {
            var t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            t.SetPixels(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);
        }

        // ---------- data access ----------

        private static string GetB64(string name)
        {
            switch (name)
            {
                case "kian_idle": return ArtData1.kian_idle;
                case "kian_walk": return ArtData1.kian_walk;
                case "kian_run": return ArtData2.kian_run;
                case "kian_jump": return ArtData2.kian_jump;
                case "kian_crouch": return ArtData2.kian_crouch;
                case "kian_hurt": return ArtData3.kian_hurt;
                case "kian_death": return ArtData3.kian_death;
                case "kian_interact": return ArtData3.kian_interact;
                case "kian_shoot": return ArtData3.kian_shoot;
                case "kian_reload": return ArtData3.kian_reload;
                case "kian_crawl": return ArtData4.kian_crawl;
                case "leila_idle": return ArtData4.leila_idle;
                case "bg_corridor": return ArtData4.bg_corridor;
                case "bg_flat": return ArtData4.bg_flat;
                case "prop_bed": return ArtData5.prop_bed;
                case "prop_wheelchair": return ArtData5.prop_wheelchair;
                case "prop_iv": return ArtData5.prop_iv;
                case "prop_cabinet": return ArtData5.prop_cabinet;
                case "prop_table": return ArtData5.prop_table;
                case "prop_switch": return ArtData6.prop_switch;
                case "prop_elevator": return ArtData6.prop_elevator;
                case "prop_celldoor": return ArtData6.prop_celldoor;
                case "prop_wristband": return ArtData6.prop_wristband;
                case "prop_folder": return ArtData6.prop_folder;
                case "prop_lamp": return ArtData6.prop_lamp;
                default: return null;
            }
        }

        private static Texture2D Tex(string name)
        {
            Texture2D t;
            if (Cache.TryGetValue(name, out t)) return t;
            string b64 = GetB64(name);
            if (string.IsNullOrEmpty(b64)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(b64);
                t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!t.LoadImage(bytes)) { UnityEngine.Object.Destroy(t); return null; }
                t.name = name;
                Cache[name] = t;
                return t;
            }
            catch (Exception e)
            {
                Debug.Log("ArtBootstrap texture failed (" + name + "): " + e.Message);
                return null;
            }
        }

        private static Sprite[] SliceSheet(string name, int frames)
        {
            Texture2D t = Tex(name);
            if (t == null || frames <= 0) return null;
            int cell = t.width / frames;
            if (cell <= 0) return null;
            var result = new Sprite[frames];
            for (int i = 0; i < frames; i++)
            {
                result[i] = Sprite.Create(t, new Rect(i * cell, 0, cell, t.height),
                    new Vector2(0.5f, 0.078f), 128f);
            }
            return result;
        }

        private static Sprite FullSprite(string name, float heightUnits, float pivotY)
        {
            Texture2D t = Tex(name);
            if (t == null || heightUnits <= 0f) return null;
            float ppu = t.height / heightUnits;
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                new Vector2(0.5f, pivotY), ppu);
        }

        private static GameObject Place(string goName, Sprite sprite, Vector3 pos, int sorting)
        {
            if (sprite == null) return null;
            var go = new GameObject(goName);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sorting;
            go.transform.position = pos;
            return go;
        }

        private static void HideOwnRenderer(GameObject go)
        {
            if (go == null) return;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }

        private static GameObject Find(string name)
        {
            try { return GameObject.Find(name); } catch { return null; }
        }

        // ---------- MainMenu ----------

        private static void ApplyMenu()
        {
            var bg = Find("Background");
            if (bg == null) return;
            Texture2D t = Tex("bg_corridor");
            if (t == null) return;
            var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
            var img = bg.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = false;
                return;
            }
            var mr = bg.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.mainTexture = t;
                mr.material = mat;
            }
        }

        // ---------- Chapter01 ----------

        private static void ApplyChapter()
        {
            // Backdrop: flat corridor plate behind everything.
            var backdrop = Place("ArtBackdrop", FullSprite("bg_flat", 12.3f, 0.5f), new Vector3(14f, 4.4f, 8f), -10);
            if (backdrop == null)
            {
                // Fallback guarantee: even if the art texture ever fails to
                // decode, the world is painted with a dark wall, never void.
                var fb = Place("ArtBackdrop", SolidSprite(new Color(0.07f, 0.09f, 0.13f, 1f)),
                    new Vector3(14f, 4.4f, 8f), -10);
                if (fb != null) fb.transform.localScale = new Vector3(10f, 3.1f, 1f);
            }

            // Hide placeholder quads (colliders and logic stay untouched).
            HideOwnRenderer(Find("Bed"));
            HideOwnRenderer(Find("Desk"));
            HideOwnRenderer(Find("Counter"));
            HideOwnRenderer(Find("Generator"));
            HideOwnRenderer(Find("ElevatorDoor"));

            // Props, bottoms on the floor (y = 0).
            Place("PropBed", FullSprite("prop_bed", 1.5f, 0f), new Vector3(-1.2f, 0f, 0f), -5);
            Place("PropIV", FullSprite("prop_iv", 2.0f, 0f), new Vector3(0.7f, 0f, 0f), -5);
            Place("PropCellDoor", FullSprite("prop_celldoor", 2.8f, 0f), new Vector3(-2.85f, 0f, 0f), -6);
            Place("PropTable", FullSprite("prop_table", 1.05f, 0f), new Vector3(5f, 0f, 0f), -5);
            Place("PropCabinet", FullSprite("prop_cabinet", 2.3f, 0f), new Vector3(9f, 0f, 0f), -5);
            Place("PropWheelchair", FullSprite("prop_wheelchair", 1.25f, 0f), new Vector3(12.3f, 0f, 0f), -5);
            Place("PropSwitchPanel", FullSprite("prop_switch", 1.9f, 0f), new Vector3(15.5f, 0f, 0f), -5);
            Place("PropElevator", FullSprite("prop_elevator", 3.3f, 0f), new Vector3(27f, 0f, 0f), -5);
            // Hanging lamp: pivot at top so it hangs from above.
            Place("PropLamp", FullSprite("prop_lamp", 2.2f, 1f), new Vector3(20f, 5.2f, 0f), -5);

            // Leila standing far down the corridor, candle in hand (idle loop).
            var leilaFrames = SliceSheet("leila_idle", 4);
            if (leilaFrames != null && leilaFrames.Length > 0)
            {
                var go = Place("NpcLeila", leilaFrames[0], new Vector3(21.5f, -0.14f, 0f), -4);
                if (go != null)
                {
                    var anim = go.AddComponent<IdleAnimator>();
                    anim.frames = leilaFrames;
                    anim.fps = 6f;
                }
            }

            // Pickup visuals.
            DressPickup("Pickup_wristband", "prop_wristband", 0.55f);
            DressPickup("Pickup_admission_form", "prop_folder", 0.6f);
            DressPickup("Pickup_nurse_note", "prop_folder", 0.6f);

            DressPlayer();
        }

        private static void DressPickup(string goName, string texName, float widthUnits)
        {
            var go = Find(goName);
            if (go == null) return;
            HideOwnRenderer(go);
            Texture2D t = Tex(texName);
            if (t == null || widthUnits <= 0f) return;
            float ppu = t.width / widthUnits;
            var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu);
            var child = new GameObject("ArtVisual");
            child.transform.SetParent(go.transform, false);
            child.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            var sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -3;
        }

        private static void DressPlayer()
        {
            var player = Find("Player");
            if (player == null) return;
            var visual = player.transform.Find("Visual");
            if (visual != null) visual.gameObject.SetActive(false);

            var idle = SliceSheet("kian_idle", 4);
            if (idle == null || idle.Length == 0) return;

            var go = new GameObject("KianSprite");
            go.transform.SetParent(player.transform, false);
            go.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = idle[0];
            sr.sortingOrder = 0;

            var anim = go.AddComponent<KianAnimator>();
            anim.idle = idle;
            anim.walk = SliceSheet("kian_walk", 12);
            anim.run = SliceSheet("kian_run", 9);
            anim.jump = SliceSheet("kian_jump", 3);
            anim.crouch = SliceSheet("kian_crouch", 4);
            anim.hurt = SliceSheet("kian_hurt", 4);
            anim.death = SliceSheet("kian_death", 5);
        }
    }
}

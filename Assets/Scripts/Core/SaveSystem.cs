using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SilentWard.Core
{
    [Serializable]
    public class InventoryEntry { public string itemId; public int count; }

    [Serializable]
    public class TouchButtonLayout { public string buttonId; public float x, y, scale, alpha; }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int chapter = 1;
        public string sceneName = "Chapter01";
        public float playerX, playerY;
        public float health = 100f;
        public float focus = 100f;
        public int skillPoints = 0;
        public List<string> unlockedSkills = new List<string>();
        public List<InventoryEntry> inventory = new List<InventoryEntry>();
        public List<string> collectedDocuments = new List<string>();
        public int difficulty = 1; // 0 easy, 1 normal, 2 hard
        // settings
        public string language = "fa";
        public float musicVolume = 0.8f, sfxVolume = 1f, ambientVolume = 0.7f;
        public int qualityLevel = 1;
        public List<TouchButtonLayout> touchLayout = new List<TouchButtonLayout>();
    }

    /// <summary>
    /// Local, fully-offline save system. Slot 0 = autosave (checkpoints),
    /// slots 1..3 = manual saves from the pause menu. JSON in persistentDataPath.
    /// </summary>
    public static class SaveSystem
    {
        private static string PathFor(int slot) =>
            Path.Combine(Application.persistentDataPath, $"silentward_save_{slot}.json");

        public static bool Save(int slot, SaveData data)
        {
            try
            {
                File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed (slot {slot}): {e.Message}");
                return false;
            }
        }

        public static SaveData Load(int slot)
        {
            try
            {
                string p = PathFor(slot);
                if (!File.Exists(p)) return null;
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(p));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed (slot {slot}): {e.Message}");
                return null;
            }
        }

        public static bool HasSave(int slot) => File.Exists(PathFor(slot));
        public static void Delete(int slot) { string p = PathFor(slot); if (File.Exists(p)) File.Delete(p); }
    }
}

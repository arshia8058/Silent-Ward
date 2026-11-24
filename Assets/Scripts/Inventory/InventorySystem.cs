using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilentWard.Inventory
{
    public enum ItemCategory { Ammo, Medicine, Document, Key, Weapon, Material }

    [CreateAssetMenu(menuName = "SilentWard/Item")]
    public class ItemData : ScriptableObject
    {
        public string id;
        public string nameKey;          // localization key
        public ItemCategory category;
        public int maxStack = 99;
        public string combinesWithId;   // optional crafting pair
        public string combineResultId;  // result of combining (e.g. alcohol+bandage -> strong bandage)
    }

    /// <summary>Capacity-managed inventory: categorized items, stacking, combining compatible items.</summary>
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private int capacitySlots = 12;
        [SerializeField] private List<ItemData> itemCatalog = new List<ItemData>();
        private readonly Dictionary<string, int> items = new();
        public event Action OnInventoryChanged;
        public IReadOnlyDictionary<string, int> Items => items;
        public int UsedSlots => items.Count;
        public int CapacitySlots => capacitySlots;

        public ItemData Find(string id) => itemCatalog.Find(i => i.id == id);

        public bool Add(string id, int count = 1)
        {
            var data = Find(id); if (data == null) return false;
            int current = items.TryGetValue(id, out var c) ? c : 0;
            if (current == 0 && items.Count >= capacitySlots) return false; // no free slot
            items[id] = Mathf.Min(data.maxStack, current + count);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool Remove(string id, int count = 1)
        {
            if (!items.TryGetValue(id, out var c) || c < count) return false;
            if (c == count) items.Remove(id); else items[id] = c - count;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool Has(string id, int count = 1) => items.TryGetValue(id, out var c) && c >= count;

        /// <summary>Combine two compatible items into their result (defined on ItemData).</summary>
        public bool Combine(string aId, string bId)
        {
            var a = Find(aId); if (a == null) return false;
            if (a.combinesWithId != bId || string.IsNullOrEmpty(a.combineResultId)) return false;
            if (!Has(aId) || !Has(bId)) return false;
            Remove(aId); Remove(bId);
            return Add(a.combineResultId);
        }

        public void UpgradeCapacity(int extra) { capacitySlots += extra; OnInventoryChanged?.Invoke(); }
        public void LoadFromSave(List<Core.InventoryEntry> saved)
        {
            items.Clear();
            foreach (var e in saved) if (Find(e.itemId) != null) items[e.itemId] = e.count;
            OnInventoryChanged?.Invoke();
        }
        public List<Core.InventoryEntry> ToSave()
        {
            var list = new List<Core.InventoryEntry>();
            foreach (var kv in items) list.Add(new Core.InventoryEntry { itemId = kv.Key, count = kv.Value });
            return list;
        }
    }
}

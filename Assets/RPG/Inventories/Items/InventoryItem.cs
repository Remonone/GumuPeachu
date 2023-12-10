﻿using System.Collections.Generic;
using RPG.Combat.Modifiers;
using RPG.Combat.Modifiers.BaseTypes;
using RPG.Inventories.Pickups;
using UnityEngine;

namespace RPG.Inventories.Items {
    
    public abstract class InventoryItem : ScriptableObject, ISerializationCallbackReceiver {
        [SerializeField] private string _itemID;
        [SerializeField] private string _itemName;
        [SerializeField] [TextArea] private string _itemDescription;
        [SerializeField] private Sprite _itemIcon;
        [SerializeField] private bool _stackable;
        [SerializeField] private Pickup _pickup;

        private static Dictionary<string, InventoryItem> _itemStore;

        public string ID => _itemID;
        public string Name => _itemName;
        public string Description => _itemDescription;
        public Sprite Icon => _itemIcon;
        public bool IsStackable => _stackable;
        
        
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if (string.IsNullOrWhiteSpace(_itemID)) {
                _itemID = System.Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        public virtual string GetDescription() {
            return _itemDescription;
        }

        public static InventoryItem GetItemByGuid(string itemID) {
            if (_itemStore == null) {
                _itemStore = new Dictionary<string, InventoryItem>();
                LoadStore();
            }
            if (itemID == null || !_itemStore.ContainsKey(itemID)) return null;
            return _itemStore[itemID];
        }

        private static void LoadStore() {
            var itemList = Resources.LoadAll<InventoryItem>("Items");
            foreach (var item in itemList) {
                if (_itemStore.ContainsKey(item._itemID)) {
                    Debug.LogError(string.Format("There's a duplicate for objects: {0} and {1}", _itemStore[item._itemID], item));
                    continue;
                }
                _itemStore[item._itemID] = item;
            }
        }
    }
}

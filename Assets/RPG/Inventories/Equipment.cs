﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RPG.Core.Predicate;
using RPG.Inventories.Items;
using RPG.Saving;
using RPG.Visuals.Display;
using UnityEngine;

namespace RPG.Inventories {
    public class Equipment : PredicateMonoBehaviour, ISaveable {

        private readonly Dictionary<EquipmentSlot, EquipmentItem> _items = new();
        private static readonly Dictionary<EquipmentSlot, GameObject> _positions = new();
        public event Action OnEquipmentChange;

        private void Start() {
            var positions = gameObject.GetComponentsInChildren<EquipmentVisualFlag>();
            foreach(var position in positions) _positions.Add(position.Slot, position.gameObject);
        }
        
        public EquipmentItem GetEquipmentItem(EquipmentSlot equipmentSlot) {
            return _items.ContainsKey(equipmentSlot) ? _items[equipmentSlot] : null;
        }
        
        public void PlaceEquipment(EquipmentItem item, EquipmentSlot equipmentSlot) {
            if (item.Slot != equipmentSlot) return;
            _items[equipmentSlot] = item;
            ApplyPredicate(_items[equipmentSlot].OnEquipPredicate);
            item.RegisterModifications(gameObject);
            DisplayItem(equipmentSlot, item);
            OnEquipmentChange?.Invoke();
        }

        public void RemoveEquipment(EquipmentSlot equipmentSlot) {
            ApplyPredicate(_items[equipmentSlot].OnUnequipPredicate);
            _items[equipmentSlot].UnregisterModifications();
            _items[equipmentSlot] = null;
            DisplayItem(equipmentSlot, null);
            OnEquipmentChange?.Invoke();
        }

        private void ApplyPredicate(EquipmentItem.Predicate predicate) {
            if (predicate.CodePredicate == "" || predicate.ComponentName == "") return;
            var formatted = 
                string.Format(predicate.CodePredicate, 
                    ((PredicateMonoBehaviour)GetComponent(predicate.ComponentName)).ComponentID);
            PredicateWorker.ParsePredicate(formatted, ComponentID);
        }
        
        public JToken CaptureAsJToken() {
            var equipmentInfo = new JProperty("equipment", new JArray(
                from equipmentID in _items 
                select new JObject(
                        new JProperty("slot", equipmentID.Key.ToString()),
                        new JProperty("id", equipmentID.Value.ID)
                    )
            ));
            return equipmentInfo;
        }
        public void RestoreFromJToken(JToken state) {
            foreach (var id in state["equipment"]) {
                var item = InventoryItem.GetItemByGuid((string)id);
                var slot = Enum.Parse<EquipmentSlot>((string)state["slot"]);
                _items[slot] = (EquipmentItem) item;
            }
        }
        public override void Predicate(string command, object[] arguments, out object result) {
            result = command switch {
                _ => null
            };
        }
        
        private void DisplayItem(EquipmentSlot slot, EquipmentItem item) {
            if (item == null || item.ItemModel == null) {
                foreach (Transform children in _positions[slot].transform) {
                    if(children.gameObject.TryGetComponent<EquipmentModel>(out _)) Destroy(children.gameObject);
                }
                return;
            }
            Instantiate(item.ItemModel, _positions[slot].transform);
        }
    }
}

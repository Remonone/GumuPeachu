﻿using RPG.Inventories;
using RPG.UI.Inventories.Slots;
using UnityEngine;

namespace RPG.UI.Inventories {
    public class InventoryUI : MonoBehaviour {

        [SerializeField] private InventorySlotUI _slotUI;

        private Inventory _inventory;

        private void Awake() {
            _inventory = Inventory.GetPlayerInventory();
        }

        private void Start() {
            _inventory.EventStorage.Subscribe("OnInventoryUpdate", RedrawUI);
            RedrawUI();
        }

        private void OnDestroy() {
            _inventory.EventStorage.Unsubscribe("OnInventoryUpdate", RedrawUI);
        }
        private void RedrawUI() {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < _inventory.Size; i++) {
                var slot = Instantiate(_slotUI, transform);
                slot.Setup(_inventory, i);
                slot.name = "Inventory Slot " + i;
            }
        }

    }
}

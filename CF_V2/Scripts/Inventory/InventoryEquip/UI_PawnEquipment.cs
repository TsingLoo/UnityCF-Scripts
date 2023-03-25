using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Inventory
{
    public class UI_PawnEquipment : MonoBehaviour
    {
        public Transform InventoryPanel;
        [HideInInspector] public bool isShowing;

        private PawnEquipment pawnEquipment;

        public Transform ItemSlotContainer;
        private List<UI_PawnEquipmentSlot> slots;

        private void Awake()
        {
            slots = GetComponentsInChildren<UI_PawnEquipmentSlot>().ToList();
            foreach (var slot in slots)
            {
                slot.OnItemRightClick += Slot_OnItemRightClick;
            }

            InventoryPanel.Hide();
        }

        private void Slot_OnItemRightClick(object sender, UI_PawnEquipmentSlot.OnItemRightClickEventArgs e)
        {
            pawnEquipment.TryUnEquipItem(e.item);
        }

        public PawnEquipment GetPawnEquipment() { return pawnEquipment; }

        public void SetPawnEquipment(PawnEquipment pawnEquipment)
        {
            this.pawnEquipment = pawnEquipment;
            UpdateVisual();

            pawnEquipment.OnEquipmentChanged += PawnEquipment_OnEquipmentChanged;
        }

        /// <summary>
        /// UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PawnEquipment_OnEquipmentChanged(object sender, System.EventArgs e)
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            foreach (var slot in slots)
            {
                Item slotItem = pawnEquipment.GetSlotItem(slot.BagPosition);
                slot.SetItem(slotItem);
            }
        }

        public void Show(bool show)
        {
            isShowing = show;

            InventoryPanel.Show(show);
        }

        // End
    }
}
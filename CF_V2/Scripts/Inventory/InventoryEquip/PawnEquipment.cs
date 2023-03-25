using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/New Equipment",
        fileName = "New Equipment")]
    public class PawnEquipment : ScriptableObject
    {
        /// <summary>
        /// UI: Reload UI
        /// WeaponsManager: Remove weapon
        /// </summary>
        public event EventHandler<OnEquipmentChangeEventArgs> OnEquipmentChanged;
        public class OnEquipmentChangeEventArgs : EventArgs
        {
            public EItemChangeType changeType;
            public Item item;
        }

        public enum EItemChangeType
        {
            Add,
            Remove
        }

        public List<Item> Items;

        private void Awake()
        {
        }

        public bool TryEquipItem(Item item, bool force = false)
        {
            var equiped = false;

            if(force 
               || !Items.Exists(it=> it.BagPosition == item.BagPosition))
            {
                equiped= true;

                Items.RemoveAll(it => it.BagPosition == item.BagPosition);
                Items.Add(item);

                OnEquipmentChanged?.Invoke(this, new OnEquipmentChangeEventArgs()
                {
                    changeType = EItemChangeType.Add,
                    item= item
                });
            }

            return equiped;
        }

        public bool TryUnEquipItem(Item item)
        {
            var removed = false;

            if (item.BagPosition != EWeaponBagPosition.Melee)
            {
                removed = true;

                Items.Remove(item);

                OnEquipmentChanged?.Invoke(this, new OnEquipmentChangeEventArgs()
                {
                    changeType = EItemChangeType.Remove,
                    item = item
                });
            }

            return removed;
        }

        public Item GetSlotItem(EWeaponBagPosition bagPosition)
        {
            return Items.FirstOrDefault(it => it.BagPosition
                == bagPosition);
        }

    }
}
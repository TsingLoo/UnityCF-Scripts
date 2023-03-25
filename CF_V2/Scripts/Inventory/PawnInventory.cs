using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.FPS.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/New Inventory",
        fileName = "New Inventory")]
    public class PawnInventory: ScriptableObject
    {
        public List<Item> itemList;
        public event EventHandler OnItemListChanged;

        public Action<Item> useItemAction;
        public Action<Item> equipItemAction;

        //public PawnInventory(List<Item> itemList,
        //    Action<Item> useItemAction,
        //    Action<Item> equipItemAction)
        //{
        //    this.itemList= itemList;

        //    this.useItemAction = useItemAction;
        //    this.equipItemAction= equipItemAction;
        //}

        // todo already in
        #region Add
        public void AddItem(Item newItem)
        {
            if (newItem.IsStackable)
            {
                var itemHave = itemList
                    .FirstOrDefault(it => it.Id == newItem.Id);
                if (itemHave != null) // already in
                {
                    itemHave.AddAmount(newItem.Amount);
                }
                else // not in
                {
                    itemList.Add(newItem);
                }
            }
            else // not stackable
            {
                itemList.Add(newItem);
            }

            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddItem(Item item, InventorySlot inventorySlot)
        {
            RemoveItem(item);

            itemList.Add(item);
            inventorySlot.SetItem(item);

            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public void RemoveItem(Item item)
        {
            if (item.IsStackable)
            {
                var itemInInventory = itemList
                    .FirstOrDefault(it => it.Id == item.Id);
                if (itemInInventory != null)
                {
                    itemInInventory.AddAmount(-item.Amount);
                    if (itemInInventory.Amount <= 0)
                    {
                        itemList.Remove(itemInInventory);
                    }
                }
            }
            else // not stackable
            {
                itemList.Remove(item);
            }

            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }


        public void UseItem(Item item)
        {
            useItemAction(item);
        }

        public void EquipItem(Item item)
        {
            equipItemAction(item);
        }


        public List<Item> GetItemList()
        {
            return itemList
                .OrderBy(it=> it.BagPosition).ToList();
        }

    }
}
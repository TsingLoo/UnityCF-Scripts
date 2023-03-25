
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.Inventory
{
    public class UI_PawnEquipmentSlot : MonoBehaviour,
        IPointerDownHandler,
        IDropHandler
    {
        public EWeaponBagPosition BagPosition;
        
        [HideInInspector] public Item item;

        #region Click

        public event EventHandler<OnItemRightClickEventArgs> OnItemRightClick;
        public class OnItemRightClickEventArgs : EventArgs
        {
            public Item item;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                if(item != null)
                {
                    OnItemRightClick?.Invoke(this,
                        new OnItemRightClickEventArgs { item = item });
                }
            }
        }
        #endregion

        // todo ref, UI_ItemSlot
        internal void SetItem(Item slotItem)
        {
            this.item = slotItem;

            InitItemSlot();
        }

        private void InitItemSlot()
        {
            // image
            var itemImage = GetComponentInChildren<RawImage>();
            itemImage.texture = null;
            itemImage.color = Color.clear;

            if(item != null)
            {
                Texture2D iconInfo = item.GetInfoImage();
                if (iconInfo != null)
                {
                    itemImage.texture = iconInfo;
                    itemImage.color = Color.white;
                }
                else
                {
                    Debug.LogWarning(item + "info icon not found");
                }
            }
        }

        #region drop
        public event EventHandler<OnItemDroppedEventArgs> OnItemDropped;
        public class OnItemDroppedEventArgs : EventArgs
        {
            public Item item;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Item item = UI_ItemDrag.Instance.GetItem();

            OnItemDropped?.Invoke(this,
                new OnItemDroppedEventArgs { item = item });
        }

        #endregion
        // End
    }
}
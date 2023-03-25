
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Unity.FPS.Inventory
{
    public class UI_Item : MonoBehaviour, IPointerDownHandler,
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private RawImage itemIcon;
        private Item item;


        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
        }

        #region Click

        public Action OnLeftClickAction;
        public Action OnRightClickAction;

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    OnLeftClickAction();
                    break;
                case PointerEventData.InputButton.Right:
                    OnRightClickAction();
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Drag Drop
        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = .5f;
            canvasGroup.blocksRaycasts = false;
            UI_ItemDrag.Instance.Show(item);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            UI_ItemDrag.Instance.Hide();
        }
        #endregion

        public void SetSprite(Texture2D texture2D)
        {
            if (texture2D != null)
            {
                itemIcon.texture = texture2D;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void SetItem(Item item)
        {
            this.item = item;
            
            // todo add
            //SetSprite(Item.GetSprite(item.itemType));
        }

        internal Item GetItem()
        {
            return this.item;
        }
    }
}
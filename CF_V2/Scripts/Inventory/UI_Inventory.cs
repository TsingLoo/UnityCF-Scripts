using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.Inventory
{
    public class UI_Inventory : MonoBehaviour
    {
        public Transform InventoryPanel;
        [HideInInspector] public bool isShowing;

        public Transform ItemSlotContainer;
        public GameObject ItemSlotTemplate;


        private PawnInventory inventory;
        //private BasePawnController player;

        private void Awake()
        {
            InventoryPanel.Hide();

            #region refer
            // get null when panel is hide
            //ItemSlotGrid = transform.DeepFind("ItemSlotGrid");
            //ItemSlotTemplate = ItemSlotGrid.DeepFind("ItemSlotTemplate");
            //ItemSlotTemplate.gameObject.SetActive(false);
            #endregion
        }

        //public void SetPlayer(BasePawnController player)
        //{
        //    this.player = player;
        //}

        public void SetInventory(PawnInventory inventory)
        {
            this.inventory = inventory;

            inventory.OnItemListChanged += Inventory_OnItemListChanged;

            RefreshInventoryItems();
        }

        private void Inventory_OnItemListChanged(object sender, System.EventArgs e)
        {
            RefreshInventoryItems();
        }

        private void RefreshInventoryItems()
        {
            foreach (Transform child in ItemSlotContainer)
            {
                //if (child == ItemSlotTemplate) continue;
                Destroy(child.gameObject);
            }

            foreach (var item in inventory.GetItemList())
            {
                // add item slot
                var newItemSlot = Instantiate(ItemSlotTemplate, ItemSlotContainer);

                var iconInfo = item.GetInfoImage();
                var itemImage = newItemSlot.GetComponentInChildren<RawImage>();
                if (iconInfo != null)
                {
                    itemImage.texture = iconInfo;
                }
                else
                {
                    itemImage.texture = null;
                    Debug.LogWarning(item + " inventory info icon not found");
                }

                newItemSlot.Show();


                #region actions
                newItemSlot.GetComponent<Button_UI>()
                    .ClickFunc = () => {
                        // Use item
                        inventory.UseItem(item);
                    };
                newItemSlot.GetComponent<Button_UI>()
                    .MouseRightClickFunc = () => {
                        // equip item
                        inventory.EquipItem(item);
                    };
                #endregion
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
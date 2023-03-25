using System;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Inventory
{
    [Serializable]
    [CreateAssetMenu(menuName = "Inventory/New Item",
        fileName = "New Item")]
    public class Item: ScriptableObject
    {
        public string Id;
        public string ItemName;
        public string AssetName;
        // Equipment position
        public EWeaponBagPosition BagPosition;

        [Header("Controller")]
        public GameObject Item1P;
        [Header("World Item")]
        public GameObject Item3P;

        // todo weapon food? armor?
        // public EItemType ItemType;


        [Header("Ammount")]
        public int Amount = 1;
        public bool IsStackable;

        [Header("Info")]
        public string Description;

        // todo ref
        public T GetComponent<T>()
        {
            return Item1P.GetComponent<T>();
        }

        internal void AddAmount(int amount)
        {
            this.Amount += amount;
        }

        public Texture2D GetInfoImage()
        {
            // image
            var asset = AssetName;
            //$"Weapons/{asset}/Icons/{asset}_Info";
            var iconInfoPath = $"UI_CF/BuySetup/Icon/BUYWEAPON_INFO_{asset}";
            
            var infoImage = Resources.Load<Texture2D>(iconInfoPath);

            return infoImage;

        }

        // End
    }
}
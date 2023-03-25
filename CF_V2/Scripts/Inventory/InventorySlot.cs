
namespace Unity.FPS.Inventory
{
    public class InventorySlot
    {

        private int index;
        private Item item;

        public InventorySlot(int index)
        {
            this.index = index;
        }

        public Item GetItem()
        {
            return item;
        }

        public void SetItem(Item item)
        {
            this.item = item;
        }

        public void RemoveItem()
        {
            item = null;
        }

        public bool IsEmpty()
        {
            return item == null;
        }

    }
}
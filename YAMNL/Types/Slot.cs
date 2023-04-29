namespace YAMNL.Types
{
    public class Slot
    {

        public Slot(Item? item, short slotNumber)
        {
            Item = item;
            SlotNumber = slotNumber;
        }

        public Item? Item { get; set; }
        public short SlotNumber { get; set; }

        public bool IsEmpty() => Item == null;
        public bool IsFull() => Item != null && Item.Count == Item.StackSize;

        /// <summary>
        /// How many items can be stacked on this slot
        /// </summary>
        public int LeftToStack => Item?.StackSize - Item?.Count ?? throw new NotSupportedException();

        public bool CanStack(Slot otherSlot, int count)
        {
            return CanStack(otherSlot.Item?.Id, count);
        }

        public bool CanStack(Slot otherSlot)
        {
            return CanStack(otherSlot.Item?.Id, otherSlot.Item?.Count);
        }

        public bool CanStack(int? itemId, int? count = null)
        {
            count ??= 1;
            if (IsEmpty() || itemId == null)
            {
                return true;
            }

            var slotType = Item!.Id;

            if (slotType == itemId)
            {

                if (Item!.StackSize == 1) return false;

                return LeftToStack >= count;

            }
            return false;
        }

        public Slot Clone() => new Slot(Item, SlotNumber);

        public override string ToString() => $"Slot (Index={SlotNumber} Item={Item?.ToString() ?? "None"})";
    }
}

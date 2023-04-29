﻿using fNbt;

namespace YAMNL.Types
{
    public class Item
    {

        public Item(int id, string displayName, string name, byte stackSize, int? maxDurability, string[]? enchantCategories, string[]? repairWith)
        {
            Id = id;
            DisplayName = displayName;
            Name = name;
            StackSize = stackSize;
            MaxDurability = maxDurability;
            EnchantCategories = enchantCategories;
            RepairWith = repairWith;
        }

        public Item(byte count, int? damage, NbtCompound? metadata, int id, string displayName, string name, byte stackSize, int? maxDurability, string[]? enchantCategories, string[]? repairWith) : this(id, displayName, name, stackSize, maxDurability, enchantCategories, repairWith)
        {
            Count = count;
            Damage = damage;
            Metadata = metadata;
        }

        public int Id { get; }
        public string DisplayName { get; }
        public string Name { get; }
        public byte StackSize { get; }
        public int? MaxDurability { get; }
        public string[]? EnchantCategories { get; }
        public string[]? RepairWith { get; }

        public byte Count { get; set; }
        public int? Damage { get; set; }
        public NbtCompound? Metadata { get; set; } // TODO: Deconstruct metadata

        public override string ToString() => $"Item (Name={Name} Id={Id} Count={Count} Metadata={(Metadata == null ? "None" : Metadata.ToString())})";


        public Slot ToSlot(short slotNumber) => new Slot(this, slotNumber);

        public Item Clone()
        {
            return new Item(
                Count, Damage, (NbtCompound?)Metadata?.Clone(), Id, DisplayName, Name, StackSize,
                MaxDurability, EnchantCategories, RepairWith);
        }

        private int GetMaterialMultiplier()
        {
            var name = Name;
            if (name.Contains("_") && (name.EndsWith("axe") || name.EndsWith("pickaxe") || name.EndsWith("hoe") || name.EndsWith("shovel") || name.EndsWith("sword")))
            {
                var material = name.Split("_")[0];
                switch (material)
                {
                    case "wooden": return 2;
                    case "stone": return 4;
                    case "iron": return 6;
                    case "diamond": return 8;
                    case "netherite": return 9;
                    case "gold": return 12;
                }
            }
            return 1;
        }

        public int GetToolMultiplier(Block block)
        {

            if (block.Material.Contains("/"))
            {
                var type = block.Material.Split("/")[1];
                if (Name.EndsWith(type)) return GetMaterialMultiplier();
            }

            return 1;
        }
    }
}

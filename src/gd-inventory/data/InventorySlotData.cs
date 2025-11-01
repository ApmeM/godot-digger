
using System;
using System.Collections.Generic;
using Godot;

public class InventorySlotData
{
    private string lootName;
    public string LootName => this.lootName;
    private int itemsCount;
    public int ItemsCount => this.itemsCount;

    public LootDefinition LootDefinition => string.IsNullOrWhiteSpace(this.lootName) ? null : LootDefinition.LootByName[this.lootName];

    public readonly HashSet<ItemType> AcceptedTypes;

    public event Action SlotContentChanged;

    public InventorySlotData(string lootName, int itemsCount, HashSet<ItemType> acceptedTypes)
    {
        ForceSetCount(lootName, itemsCount);
        this.AcceptedTypes = acceptedTypes;
    }

    public InventorySlotData(InventorySlotData a)
    {
        ForceSetCount(a.LootName, a.ItemsCount);
        this.AcceptedTypes = new HashSet<ItemType>(a.AcceptedTypes);
    }

    public InventorySlotData()
    {
        ForceSetCount(string.Empty, 0);
        this.AcceptedTypes = new HashSet<ItemType>();
    }

    public bool HasItem()
    {
        return ItemsCount > 0;
    }

    public void ClearItem()
    {
        this.ForceSetCount("", 0);
    }

    public void ForceSetCount(string lootName, int itemsCount)
    {
        if (string.IsNullOrWhiteSpace(LootName) && ItemsCount > 0)
        {
            throw new Exception($"Inventory slot state inconsistent: LootName = {LootName}, ItemsCount = {ItemsCount}");
        }

        this.itemsCount = itemsCount;
        this.lootName = lootName;
        this.SlotContentChanged?.Invoke();
    }

    public int TryChangeCount(string lootName, int countDiff)
    {
        var loot = LootDefinition.LootByName[lootName];

        if (this.AcceptedTypes.Count > 0 && !this.AcceptedTypes.Contains(loot.ItemType))
        {
            GD.PrintErr($"This slot does not accept this item type. ItemTypes: {loot.ItemType}, AcceptedType: {string.Join(",", this.AcceptedTypes)}");
            return countDiff;
        }

        if (HasItem() && this.LootName != lootName)
        {
            GD.PrintErr("Can not add loot item to the slot as it is already occupied by different resouce.");
            return countDiff;
        }

        if (!HasItem() && countDiff < 0)
        {
            return countDiff;
        }

        var result = this.ItemsCount + countDiff;
        if (result <= 0)
        {
            ClearItem();
            return result;
        }

        var count = Math.Min(result, loot.MaxCount);
        result -= count;

        ForceSetCount(lootName, count);

        return result;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BagInventoryData
{
    public event Action SlotContentChanged;
    public void IntenralSlotContentChanged()
    {
        SlotContentChanged?.Invoke();
    }

    public InventorySlotData Bag = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Bag });
    public InventorySlotData Money = new InventorySlotData(string.Empty, 0, new HashSet<ItemType> { ItemType.Money });
    public EquipmentInventoryData Equipment = new EquipmentInventoryData();
    public InventoryData Inventory = new InventoryData();

    public BagInventoryData()
    {
        this.Bag.SlotContentChanged += IntenralSlotContentChanged;
        this.Money.SlotContentChanged += IntenralSlotContentChanged;
        this.Equipment.SlotContentChanged += IntenralSlotContentChanged;
        this.Inventory.SlotContentChanged += IntenralSlotContentChanged;
    }

    public int TryChangeCount(string lootName, int count)
    {
        var loot = LootDefinition.LootByName[lootName];
        if (loot.ItemType == ItemType.Money)
        {
            Money.ForceSetCount(lootName, Money.ItemsCount + count);

            return 0;
        }

        return this.Inventory.TryChangeCount(lootName, count);
    }

    public int GetItemCount(string lootName)
    {
        var loot = LootDefinition.LootByName[lootName];
        if (loot.ItemType == ItemType.Money)
        {
            return Money.ItemsCount;
        }

        return this.Inventory.GetItemCount(lootName);
    }

    public bool TryChangeCountsOrCancel(IEnumerable<(string, int)> enumerable)
    {
        return this.Inventory.TryChangeCountsOrCancel(enumerable);
    }

    public void ApplyEquipment(BaseUnit.EffectiveCharacteristics character)
    {
        var equipments = this.Equipment.GetItems()
            .Where(a => a.HasItem())
            .Select(a => a.LootDefinition)
            .ToList();

        foreach (var loot in equipments)
        {
            loot.EquipAction?.Invoke(character);
        }

        var inventories = this.Inventory.Slots
            .Where(a => a.HasItem())
            .Select(a => a.LootDefinition)
            .ToList();

        foreach (var loot in inventories)
        {
            loot.InventoryAction?.Invoke(character);
        }

        if (this.Bag.HasItem())
        {
            this.Bag.LootDefinition.EquipAction?.Invoke(character);
        }
    }
}

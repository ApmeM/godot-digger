using System.Collections.Generic;
using System.Linq;
using Godot;

[SceneReference("BagInventoryPopup.tscn")]
public partial class BagInventoryPopup
{
    public InventorySlot Bag => this.bagSlot;
    public InventorySlot Money => this.moneySlot;
    public EquipmentInventory Equipment => this.equipmentInventory;
    public Inventory BagInventory => this.bagInventory;

    [Signal]
    public delegate void SlotItemDoubleClicked(InventorySlot slot);

    [Signal]
    public delegate void SlotItemRightClicked(InventorySlot slot);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.bagInventory.Connect(nameof(Inventory.DragOnAnotherItemType), this, nameof(InventoryTryMergeItems));
        this.bagInventory.Connect(nameof(Inventory.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
        this.bagInventory.Connect(nameof(Inventory.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        this.bagInventory.Connect(nameof(Inventory.SlotContentChanged), this, nameof(SlotContentChangedHandler));
        this.bagInventory.Connect(nameof(Inventory.SlotsCountChanged), this, nameof(SlotsCountChangedHandler));
        this.equipmentInventory.Connect(nameof(EquipmentInventory.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
        this.equipmentInventory.Connect(nameof(EquipmentInventory.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        this.equipmentInventory.Connect(nameof(EquipmentInventory.SlotContentChanged), this, nameof(SlotContentChangedHandler));
        this.bagSlot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
        this.bagSlot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        this.bagSlot.Connect(nameof(InventorySlot.SlotContentChanged), this, nameof(SlotContentChangedHandler));
        this.moneySlot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
        this.moneySlot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        this.moneySlot.Connect(nameof(InventorySlot.SlotContentChanged), this, nameof(SlotContentChangedHandler));
    }

    private void SlotItemDoubleClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemDoubleClicked), slot);
    }
    private void SlotItemRightClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemRightClicked), slot);
    }

    protected void InventoryTryMergeItems(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (!toSlot.LootDefinition.MergeActions.ContainsKey(fromSlot.LootName))
        {
            return;
        }

        var mergeResult = toSlot.LootDefinition.MergeActions[fromSlot.LootName];
        fromSlot.TryChangeCount(fromSlot.LootName, -1);
        toSlot.TryChangeCount(toSlot.LootName, -1);

        toSlot.TryChangeCount(mergeResult, 1);
    }

    public void ConfigureInventory(Inventory newInventory)
    {
        newInventory.GetParent().RemoveChild(newInventory);
        this.differentInventoriesContainer.AddChild(newInventory);
    }

    #region Data

    [Signal]
    public delegate void SlotContentChanged(InventorySlot slot);
    private void SlotContentChangedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotContentChanged), slot);
    }

    [Signal]
    public delegate void SlotsCountChanged(Inventory slot);
    private void SlotsCountChangedHandler()
    {
        this.EmitSignal(nameof(SlotsCountChanged), this);
    }

    public int TryChangeCount(string lootName, int count)
    {
        var loot = LootDefinition.LootByName[lootName];
        if (loot.ItemType == ItemType.Money)
        {
            this.moneySlot.ForceSetCount(lootName, this.moneySlot.ItemsCount + count);

            return 0;
        }

        return this.bagInventory.TryChangeCount(lootName, count);
    }

    public int GetItemCount(string lootName)
    {
        var loot = LootDefinition.LootByName[lootName];
        if (loot.ItemType == ItemType.Money)
        {
            return this.moneySlot.ItemsCount;
        }

        return this.bagInventory.GetItemCount(lootName);
    }

    public bool TryChangeCountsOrCancel(IEnumerable<(string, int)> enumerable)
    {
        return this.bagInventory.TryChangeCountsOrCancel(enumerable);
    }

    public void UpdateSlotsCount(int value)
    {
        this.bagInventory.UpdateSlotsCount(value);
    }

    public void ApplyEquipment(BaseUnit.EffectiveCharacteristics character)
    {
        var equipments = this.equipmentInventory.GetItems()
            .Where(a => a.HasItem())
            .Select(a => a.LootDefinition)
            .ToList();

        foreach (var loot in equipments)
        {
            loot.EquipAction?.Invoke(character);
        }

        var inventories = this.bagInventory.GetItems()
            .Where(a => a.HasItem())
            .Select(a => a.LootDefinition)
            .ToList();

        foreach (var loot in inventories)
        {
            loot.InventoryAction?.Invoke(character);
        }

        if (this.bagSlot.HasItem())
        {
            this.bagSlot.LootDefinition.EquipAction?.Invoke(character);
        }
    }
    #endregion
}

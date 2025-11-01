using Godot;

[SceneReference("BagInventoryPopup.tscn")]
public partial class BagInventoryPopup
{
    private BagInventoryData slotData = new BagInventoryData();
    public BagInventoryData SlotData
    {
        get => slotData;
        set
        {
            if (slotData == value)
            {
                return;
            }
            slotData = value;
            RefreshFromDump();
        }
    }

    [Signal]
    public delegate void UseItem(InventorySlot slot);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.bagInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
        this.bagInventory.Connect(nameof(Inventory.DragOnAnotherItemType), this, nameof(InventoryTryMergeItems));

        RefreshFromDump();
    }

    protected void InventoryUseItem(InventorySlot slot)
    {
        this.EmitSignal(nameof(UseItem), slot);
    }

    protected void InventoryTryMergeItems(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (!toSlot.SlotData.LootDefinition.MergeActions.ContainsKey(fromSlot.SlotData.LootName))
        {
            return;
        }

        var mergeResult = toSlot.SlotData.LootDefinition.MergeActions[fromSlot.SlotData.LootName];
        fromSlot.SlotData.TryChangeCount(fromSlot.SlotData.LootName, -1);
        toSlot.SlotData.TryChangeCount(toSlot.SlotData.LootName, -1);

        toSlot.SlotData.TryChangeCount(mergeResult, 1);
    }

    private void RefreshFromDump()
    {
        this.moneySlot.SlotData = this.slotData.Money;
        this.bagSlot.SlotData = this.slotData.Bag;
        this.equipmentInventory.SlotData = this.slotData.Equipment;
        this.bagInventory.SlotData = this.slotData.Inventory;
    }

    public void ConfigureInventory(Inventory newInventory)
    {
        newInventory.GetParent().RemoveChild(newInventory);
        this.differentInventoriesContainer.AddChild(newInventory);
    }
}

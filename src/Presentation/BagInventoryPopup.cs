using System.Collections.Generic;
using System.Linq;
using Godot;

[SceneReference("BagInventoryPopup.tscn")]
public partial class BagInventoryPopup
{
    [Signal]
    public delegate void BagChanged(int itemId, int from, int to);

    [Signal]
    public delegate void EquipmentChanged(InventorySlot slot, int itemId, int from, int to);

    [Signal]
    public delegate void UseItem(InventorySlot slot);

    public uint Size { get => this.bagInventory.Size; set => this.bagInventory.Size = value; }

    public Dictionary<int, InventorySlot.InventorySlotConfig> Config = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        foreach (var id in LootDefinition.LootByName)
        {
            Config.Add((int)id.Value.Id, new InventorySlot.InventorySlotConfig
            {
                Texture = id.Value.Image,
                MaxCount = id.Value.MaxCount,
                ItemType = (int)id.Value.ItemType
            });
        }

        this.bagInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
        this.bagInventory.Connect(nameof(Inventory.DragOnAnotherItemType), this, nameof(InventoryTryMergeItems));
        this.bagSlot.Connect(nameof(InventorySlot.ItemCountChanged), this, nameof(BagSlotChanged));
        this.equipmentInventory.Connect(nameof(EquipmentInventory.ItemCountChanged), this, nameof(EquipmentInventoryChanged));

        this.equipmentInventory.Config = Config;

        this.bagInventory.Config = Config;

        this.bagSlot.AcceptedTypes.Add((int)ItemType.Bag);
        this.bagSlot.Config = Config;

        this.moneySlot.AcceptedTypes.Add((int)ItemType.Money);
        this.moneySlot.Config = Config;
    }


    private void EquipmentInventoryChanged(InventorySlot slot, int itemId, int from, int to)
    {
        this.EmitSignal(nameof(EquipmentChanged), slot, itemId, from, to);
    }

    private void BagSlotChanged(int itemId, int from, int to)
    {
        this.EmitSignal(nameof(BagChanged), itemId, from, to);
    }

    protected void InventoryUseItem(InventorySlot slot)
    {
        this.EmitSignal(nameof(UseItem), slot);
    }

    protected void InventoryTryMergeItems(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (!LootDefinition.LootById[toSlot.ItemId].MergeActions.ContainsKey(LootDefinition.LootById[fromSlot.ItemId].Name))
        {
            return;
        }

        var mergeResult = LootDefinition.LootById[toSlot.ItemId].MergeActions[LootDefinition.LootById[fromSlot.ItemId].Name];
        fromSlot.TryChangeCount(fromSlot.ItemId, -1);
        toSlot.TryChangeCount(toSlot.ItemId, -1);

        toSlot.TryChangeCount(LootDefinition.LootByName[mergeResult].Id, 1);
    }

    public void LoadInventoryDump(InventoryDump inventoryDump)
    {
        this.bagSlot.ClearItem();
        this.equipmentInventory.ClearItems();
        this.bagInventory.ClearItems();

        if (inventoryDump == null)
        {
            return;
        }

        if (inventoryDump.Bag != default)
        {
            this.bagSlot.ForceSetCount(inventoryDump.Bag.Item1, inventoryDump.Bag.Item2);
        }

        if (inventoryDump.Equipment != null)
        {
            this.equipmentInventory.ForceSetItems(inventoryDump.Equipment);
        }

        if (inventoryDump.Inventory != null)
        {
            this.bagInventory.SetItems(inventoryDump.Inventory);
        }
    }


    public InventoryDump GetInventoryDump()
    {
        return new InventoryDump
        {
            Bag = this.bagSlot.GetItem(),
            Equipment = this.equipmentInventory.GetItems(),
            Inventory = this.bagInventory.GetItems(),
        };
    }

    public void ConfigureInventory(Inventory newInventory)
    {
        newInventory.GetParent().RemoveChild(newInventory);
        this.differentInventoriesContainer.AddChild(newInventory);
    }

    public int TryChangeCount(string lootId, int count)
    {
        var loot = LootDefinition.LootByName[lootId];
        if (loot.ItemType == ItemType.Money)
        {
            if (moneySlot.TryChangeCount(loot.Id, count) == 0)
            {
                return 0;
            }
        }

        return this.bagInventory.TryChangeCount(loot.Id, count);
    }

    public int GetItemCount(string lootId)
    {
        var loot = LootDefinition.LootByName[lootId];
        if (loot.ItemType == ItemType.Money)
        {
            return moneySlot.GetItem().Item2;
        }
        
        return this.bagInventory.GetItemCount(loot.Id);
    }

    public void ApplyEquipment(Character character)
    {
        character.BagId = this.bagSlot.ItemId == -1 ? null : LootDefinition.LootById[this.bagSlot.ItemId];
        character.WeaponId = this.equipmentInventory.WeaponId == -1 ? null : LootDefinition.LootById[this.equipmentInventory.WeaponId];

        var loots = this.equipmentInventory.GetItems()
            .Where(a => a.Item1 >= 0)
            .Select(a => LootDefinition.LootById[a.Item1])
            .ToList();

        foreach (var loot in loots)
        {
            loot.EquipAction(character);
        }

        character.BagId?.EquipAction(character);
    }

    internal bool TryChangeCountsOrCancel(IEnumerable<(string, int)> enumerable)
    {
        return this.bagInventory.TryChangeCountsOrCancel(enumerable.Select(a => (LootDefinition.LootByName[a.Item1].Id, a.Item2)));
    }
}

using System.Collections.Generic;
using Godot;

[SceneReference("BagInventoryPopup.tscn")]
public partial class BagInventoryPopup
{
    [Export]
    public TileSet LootTileSet;

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

        foreach (int id in this.LootTileSet.GetTilesIds())
        {
            var tex = this.LootTileSet.TileGetTexture(id);
            var definition = LootDefinition.KnownLoot[(id, 0, 0)];

            Config.Add(id, new InventorySlot.InventorySlotConfig
            {
                Texture = tex,
                MaxCount = definition.MaxCount,
                ItemType = (int)definition.ItemType
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
        if (!LootDefinition.KnownLoot[(toSlot.ItemId, 0, 0)].MergeActions.ContainsKey((fromSlot.ItemId, 0, 0)))
        {
            return;
        }

        var mergeResult = LootDefinition.KnownLoot[(toSlot.ItemId, 0, 0)].MergeActions[(fromSlot.ItemId, 0, 0)];
        fromSlot.TryChangeCount(fromSlot.ItemId, -1);
        toSlot.TryChangeCount(toSlot.ItemId, -1);

        toSlot.TryChangeCount(mergeResult.Item1, 1);
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

    public int TryChangeCount(int lootId, int count)
    {
        return this.bagInventory.TryChangeCount(lootId, count);
    }

    public int GetItemCount(int lootId)
    {
        return this.bagInventory.GetItemCount(lootId);
    }

    public void ApplyEquipment(Character character)
    {
        character.BagId = this.bagSlot.ItemId;
        character.WeaponId = this.equipmentInventory.WeaponId;
        this.equipmentInventory.ApplyEquipment(character);
        if (this.bagSlot.ItemId >= 0)
        {
            var definition = LootDefinition.KnownLoot[(this.bagSlot.ItemId, 0, 0)];
            character.DigPower += (uint)definition.DigPower;
            character.MaxStamina += (uint)definition.NumberOfTurns;
            character.BagSlots += (uint)definition.AdditionalSlots;
        }
    }

    internal bool TryChangeCountsOrCancel(IEnumerable<(int, int)> enumerable)
    {
        return this.bagInventory.TryChangeCountsOrCancel(enumerable);
    }
}

using Godot;
using System;
using System.Linq;

[SceneReference("EquipmentInventory.tscn")]
public partial class EquipmentInventory
{

    [Signal]
    public delegate void ItemCountChanged(InventorySlot slot, int itemId, int from, int to);

    private Inventory.InventoryConfig config;

    public Inventory.InventoryConfig Config
    {
        get => config;
        set
        {
            config = value;
            if (IsInsideTree())
            {
                foreach (InventorySlot slot in this.GetChildren().OfType<InventorySlot>())
                {
                    slot.Config = value;
                }
            }
        }
    }

    public override void _Ready()
    {
        this.neckSlot.AcceptedTypes.Add((int)ItemType.Neck);
        this.helmSlot.AcceptedTypes.Add((int)ItemType.Helm);
        this.weaponSlot.AcceptedTypes.Add((int)ItemType.Weapon);
        this.chestSlot.AcceptedTypes.Add((int)ItemType.Chest);
        this.shieldSlot.AcceptedTypes.Add((int)ItemType.Shield);
        this.ring1Slot.AcceptedTypes.Add((int)ItemType.Ring);
        this.beltSlot.AcceptedTypes.Add((int)ItemType.Belt);
        this.ring2Slot.AcceptedTypes.Add((int)ItemType.Ring);
        this.pantsSlot.AcceptedTypes.Add((int)ItemType.Pants);
        this.bootsSlot.AcceptedTypes.Add((int)ItemType.Boots);

        this.Config = this.config;

        foreach (InventorySlot slot in this.GetChildren().OfType<InventorySlot>())
        {
            slot.Connect(nameof(InventorySlot.ItemCountChanged), this, nameof(SlotItemCountChanged), new Godot.Collections.Array { slot });
        }
    }

    private void SlotItemCountChanged(int itemId, int from, int to, InventorySlot slot)
    {
        this.cacheDigPower = -1;
        this.cacheNumberOfTurns = -1;
        this.EmitSignal(nameof(ItemCountChanged), slot, itemId, from, to);
    }

    private int cacheDigPower = -1;
    public uint CalcDigPower()
    {
        if (cacheDigPower != -1)
        {
            return (uint)cacheDigPower;
        }

        cacheDigPower = 1 + Math.Max(0, this.GetChildren().OfType<InventorySlot>().Where(a => a.ItemId >= 0).Select(a => LootDefinition.KnownLoot[(a.ItemId, 0, 0)].DigPower).Sum());
        return (uint)cacheDigPower;
    }

    private int cacheNumberOfTurns = -1;
    public uint CalcNumberOfTurns()
    {
        if (cacheNumberOfTurns != -1)
        {
            return (uint)cacheNumberOfTurns;
        }

        cacheNumberOfTurns = 10 + Math.Max(0, this.GetChildren().OfType<InventorySlot>().Where(a => a.ItemId >= 0).Select(a => LootDefinition.KnownLoot[(a.ItemId, 0, 0)].NumberOfTurns).Sum());
        return (uint)cacheNumberOfTurns;
    }
}

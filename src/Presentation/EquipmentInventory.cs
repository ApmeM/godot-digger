using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[SceneReference("EquipmentInventory.tscn")]
public partial class EquipmentInventory
{
    [Signal]
    public delegate void ItemCountChanged(InventorySlot slot, int itemId, int from, int to);

    public Dictionary<int, InventorySlot.InventorySlotConfig> config = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public Dictionary<int, InventorySlot.InventorySlotConfig> Config
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
        this.EmitSignal(nameof(ItemCountChanged), slot, itemId, from, to);
    }

    public void ApplyEquipment(Character character)
    {
        character.DigPower = (uint)(character.DigPower + Math.Max(0, this.GetChildren().OfType<InventorySlot>().Where(a => a.ItemId >= 0).Select(a => LootDefinition.KnownLoot[(a.ItemId, 0, 0)].DigPower).Sum()));
        character.MaxStamina = (uint)(character.MaxStamina + Math.Max(0, this.GetChildren().OfType<InventorySlot>().Where(a => a.ItemId >= 0).Select(a => LootDefinition.KnownLoot[(a.ItemId, 0, 0)].NumberOfTurns).Sum()));
    }

    public List<(int, int)> GetItems()
    {
        return this.GetChildren()
            .OfType<InventorySlot>()
            .Select(a => a.GetItem())
            .ToList();
    }

    public void SetItems(List<(int, int)> items)
    {
        var slots = this.GetChildren().OfType<InventorySlot>().ToList();
        for (var i = 0; i < items.Count; i++)
        {
            slots[i].ForceSetCount(items[i].Item1, items[i].Item2);
        }
    }
}

using Godot;
using System;
using System.Linq;

[SceneReference("EquipmentInventory.tscn")]
public partial class EquipmentInventory
{
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

        var slots = new[]{
            this.neckSlot,
            this.helmSlot,
            this.weaponSlot,
            this.chestSlot,
            this.shieldSlot,
            this.ring1Slot,
            this.beltSlot,
            this.ring2Slot,
            this.pantsSlot,
            this.bootsSlot,
        };

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
    }

    public uint CalcDigPower()
    {
        return 1 + (uint)Math.Max(0, this.GetChildren().OfType<InventorySlot>().Where(a => a.ItemId >= 0).Select(a => LootDefinition.KnownLoot[(a.ItemId, 0, 0)].DigPower).Sum());
    }

    public uint CalcNumberOfTurns()
    {
        return 10;
    }
}

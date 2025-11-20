using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[SceneReference("EquipmentInventory.tscn")]
public partial class EquipmentInventory
{
    [Signal]
    public delegate void SlotItemDoubleClicked(InventorySlot slot);

    [Signal]
    public delegate void SlotItemRightClicked(InventorySlot slot);

    [Signal]
    public delegate void SlotContentChanged(InventorySlot slot);

    private void SlotItemDoubleClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemDoubleClicked), slot);
    }
    private void SlotItemRightClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemRightClicked), slot);
    }
    private void SlotContentChangedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotContentChanged), slot);
    }

    private InventorySlot[] All;

    public InventorySlot Neck => this.neckSlot;
    public InventorySlot Helm => this.helmSlot;
    public InventorySlot Weapon => this.weaponSlot;
    public InventorySlot Chest => this.chestSlot;
    public InventorySlot Shield => this.shieldSlot;
    public InventorySlot Ring1 => this.ring1Slot;
    public InventorySlot Belt => this.beltSlot;
    public InventorySlot Ring2 => this.ring2Slot;
    public InventorySlot Pants => this.pantsSlot;
    public InventorySlot Boots => this.bootsSlot;

    public override void _Ready()
    {
        this.All = new[]{
            this.neckSlot,
            this.helmSlot,
            this.weaponSlot,
            this.chestSlot,
            this.shieldSlot,
            this.ring1Slot,
            this.beltSlot,
            this.ring2Slot,
            this.pantsSlot,
            this.bootsSlot
        };

        foreach (var slot in All)
        {
            slot.Connect(nameof(InventorySlot.SlotContentChanged), this, nameof(SlotContentChangedHandler));
            slot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
            slot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        }
    }

    #region Data

    public List<InventorySlot> GetItems()
    {
        return All.ToList();
    }

    public void ClearItems()
    {
        foreach (var slot in All)
        {
            if (!slot.HasItem())
            {
                continue;
            }

            slot.ClearItem();
        }
    }

    public void ForceSetItems(IEnumerable<(string, int)> items)
    {
        this.ClearItems();
        foreach (var item in items)
        {
            var loot = LootDefinition.LootByName[item.Item1];
            switch (loot.ItemType)
            {
                case ItemType.Neck: this.neckSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Helm: this.helmSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Weapon: this.weaponSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Chest: this.chestSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Shield: this.shieldSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Ring:
                    {
                        if (!this.ring1Slot.HasItem())
                        {
                            this.ring1Slot.ForceSetCount(item.Item1, item.Item2);
                        }
                        else
                        {
                            this.ring2Slot.ForceSetCount(item.Item1, item.Item2);
                        }
                    }
                    break;
                case ItemType.Belt: this.beltSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Pants: this.pantsSlot.ForceSetCount(item.Item1, item.Item2); break;
                case ItemType.Boots: this.bootsSlot.ForceSetCount(item.Item1, item.Item2); break;
            }
        }
    }
    #endregion
}

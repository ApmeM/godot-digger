using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static InventorySlot;

[SceneReference("Inventory.tscn")]
[Tool]
public partial class Inventory
{
    public Dictionary<int, InventorySlot.InventorySlotConfig> config = new Dictionary<int, InventorySlot.InventorySlotConfig>();

    public Dictionary<int, InventorySlot.InventorySlotConfig> Config
    {
        get => config;
        set
        {
            config = value;
            if (IsInsideTree())
            {
                foreach (InventorySlot slot in this.slotContainer.GetChildren().Cast<InventorySlot>())
                {
                    slot.Config = value;
                }
            }
        }
    }

    private DropOnAnotherItemTypeAction dropOnAnotherItemType = DropOnAnotherItemTypeAction.Not_Allowed;

    [Export]
    public DropOnAnotherItemTypeAction DropOnAnotherItemType
    {
        get => dropOnAnotherItemType;
        set
        {
            dropOnAnotherItemType = value;
            if (IsInsideTree())
            {
                foreach (InventorySlot slot in this.slotContainer.GetChildren().Cast<InventorySlot>())
                {
                    slot.DropOnAnotherItemType = value;
                }
            }
        }
    }

    [Export]
    public PackedScene InventorySlotScene;

    [Signal]
    public delegate void UseItem(InventorySlot slot);

    [Signal]
    public delegate void DragOnAnotherItemType(InventorySlot from, InventorySlot to);

    [Signal]
    public delegate void ItemCountChanged(InventorySlot slot, int itemId, int from, int to);

    private uint size;

    [Export]
    public uint Size
    {
        get
        {
            return this.size;
        }
        set
        {
            if (IsInsideTree())
            {
                if (this.slotContainer.GetChildCount() < value)
                {
                    var toAdd = value - this.slotContainer.GetChildCount();
                    for (var i = 0; i < toAdd; i++)
                    {
                        var slot = this.InventorySlotScene.Instance<InventorySlot>();
                        slot.Config = Config;
                        this.slotContainer.AddChild(slot);
                        slot.Connect(nameof(InventorySlot.UseItem), this, nameof(SlotUseItem), new Godot.Collections.Array { slot });
                        slot.Connect(nameof(InventorySlot.DragOnAnotherItemType), this, nameof(SlotDragOnAnotherItemType));
                        slot.Connect(nameof(InventorySlot.ItemCountChanged), this, nameof(SlotItemCountChanged), new Godot.Collections.Array { slot });
                    }
                }
                else if (this.slotContainer.GetChildCount() > value)
                {
                    var toDelete = this.slotContainer.GetChildCount() - value;
                    var emptyList = this.slotContainer.GetChildren().OfType<InventorySlot>().Where(a => !a.HasItem()).ToList();
                    var deleteEmpty = Math.Min(toDelete, emptyList.Count);
                    var deleteNonEmpty = toDelete - deleteEmpty;

                    for (var i = 0; i < deleteEmpty; i++)
                    {
                        this.slotContainer.RemoveChild(emptyList[i]);
                        emptyList[i].QueueFree();
                    }

                    for (var i = 0; i < deleteNonEmpty - value; i++)
                    {
                        GD.Print("Forced to remove slot with items.");
                        var child = this.slotContainer.GetChild(0);
                        this.slotContainer.RemoveChild(child);
                        child.QueueFree();
                    }
                }
            }
            this.size = value;
        }
    }

    private void SlotItemCountChanged(int itemId, int from, int to, InventorySlot slot)
    {
        this.EmitSignal(nameof(ItemCountChanged), slot, itemId, from, to);
    }

    private void SlotUseItem(InventorySlot slot)
    {
        this.EmitSignal(nameof(UseItem), slot);
    }

    private void SlotDragOnAnotherItemType(InventorySlot from, InventorySlot to)
    {
        this.EmitSignal(nameof(DragOnAnotherItemType), from, to);
    }

    private int sizePerRow;

    [Export]
    public int SizePerRow
    {
        get => this.sizePerRow;
        set
        {
            if (IsInsideTree())
            {
                this.slotContainer.Columns = value;
            }
            this.sizePerRow = value;
        }
    }

    private string title;

    [Export]
    public string Title
    {
        get => this.title;
        set
        {
            if (IsInsideTree())
            {
                this.inventoreNameLabel.Text = value;
            }
            this.title = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.SizePerRow = this.sizePerRow;
        this.Size = size;
        this.Title = this.title;
        this.Config = this.config;
        this.DropOnAnotherItemType = this.dropOnAnotherItemType;
    }

    public int TryChangeCount(int itemId, int countDiff)
    {
        if (countDiff == 0)
        {
            return 0;
        }

        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            countDiff = slot.TryChangeCount(itemId, countDiff);
            if (countDiff == 0)
            {
                return 0;
            }
        }

        return countDiff;
    }

    public bool TryChangeCountsOrCancel(IEnumerable<(int, int)> items)
    {
        var before = this.slotContainer.GetChildren()
            .OfType<InventorySlot>()
            .Where(a => a.HasItem())
            .Select(a => new { slot = a, item = a.GetItem() })
            .ToList();

        foreach (var item in items)
        {
            var result = this.TryChangeCount(item.Item1, item.Item2);
            if (result != 0)
            {
                foreach (var restore in before)
                {
                    restore.slot.ForceSetCount(restore.item.Item1, restore.item.Item2);
                }
                return false;
            }
        }

        return true;
    }

    public void ClearItems()
    {
        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            if (!slot.HasItem())
            {
                continue;
            }

            slot.ClearItem();
        }
    }

    public List<(int, int)> GetItems()
    {
        return this.slotContainer.GetChildren()
            .OfType<InventorySlot>()
            .Select(a => a.GetItem())
            .ToList();
    }

    public void SetItems(List<(int, int)> items)
    {
        var slots = this.slotContainer.GetChildren().OfType<InventorySlot>().ToList();
        for (var i = 0; i < Math.Min(items.Count, slots.Count); i++)
        {
            slots[i].ForceSetCount(items[i].Item1, items[i].Item2);
        }
    }

    public int GetItemCount(int item)
    {
        return this.GetItems()
            .Where(a => a.Item1 == item)
            .Sum(a => a.Item2);
    }
}

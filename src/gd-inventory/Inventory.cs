using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static InventorySlot;

[SceneReference("Inventory.tscn")]
[Tool]
public partial class Inventory
{
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
    public delegate void SlotItemDoubleClicked(InventorySlot slot);

    [Signal]
    public delegate void SlotItemRightClicked(InventorySlot slot);

    [Signal]
    public delegate void DragOnAnotherItemType(InventorySlot from, InventorySlot to);

    private uint size;

    [Export]
    public uint Size
    {
        get => size;
        set
        {
            if (IsInsideTree())
            {
                this.UpdateSlotsCount((int)value);
            }
            this.size = value;
        }
    }

    private void SlotItemDoubleClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemDoubleClicked), slot);
    }
    private void SlotItemRightClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemRightClicked), slot);
    }
    private void SlotDragOnAnotherItemTypeHandler(InventorySlot from, InventorySlot to)
    {
        this.EmitSignal(nameof(DragOnAnotherItemType), from, to);
    }

    private int sizePerRow = 1;

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
        this.Size = this.size;
        this.Title = this.title;
        this.DropOnAnotherItemType = this.dropOnAnotherItemType;

        for (var i = 0; i < this.slotContainer.GetChildCount(); i++)
        {
            var slot = this.slotContainer.GetChild<InventorySlot>(i);
            if (!slot.IsConnected(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler)))
            {
                slot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
                slot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
                slot.Connect(nameof(InventorySlot.DragOnAnotherItemType), this, nameof(SlotDragOnAnotherItemTypeHandler));
                slot.Connect(nameof(InventorySlot.SlotContentChanged), this, nameof(SlotContentChangedHandler));
            }
        }
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

    public void UpdateSlotsCount(int value)
    {
        if (value == this.slotContainer.GetChildCount())
        {
            return;
        }

        var toAdd = Math.Max(0, value - this.slotContainer.GetChildCount());
        for (var i = 0; i < toAdd; i++)
        {
            var slot = this.InventorySlotScene.Instance<InventorySlot>();
            this.slotContainer.AddChild(slot);
            if (!slot.IsConnected(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler)))
            {
                slot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
                slot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
                slot.Connect(nameof(InventorySlot.DragOnAnotherItemType), this, nameof(SlotDragOnAnotherItemTypeHandler));
                slot.Connect(nameof(InventorySlot.SlotContentChanged), this, nameof(SlotContentChangedHandler));
            }
        }

        var toDelete = Math.Max(0, this.slotContainer.GetChildCount() - value);
        var emptyChildren = this.slotContainer.GetChildren().Cast<InventorySlot>().OrderBy(a => a.HasItem()).Take(toDelete).ToList();
        foreach (var child in emptyChildren)
        {
            this.slotContainer.RemoveChild(child);
            child.QueueFree();
        }

        this.SlotsCountChangedHandler();
    }

    public int TryChangeCount(string lootName, int countDiff)
    {
        if (countDiff == 0)
        {
            return 0;
        }

        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            countDiff = slot.TryChangeCount(lootName, countDiff);
            if (countDiff == 0)
            {
                return 0;
            }
        }

        return countDiff;
    }

    public int GetItemCount(string lootName)
    {
        return this.slotContainer.GetChildren()
            .Cast<InventorySlot>()
            .Where(a => a.LootName == lootName)
            .Sum(a => a.ItemsCount);
    }

    public void ClearItems()
    {
        foreach (var cell in this.slotContainer.GetChildren().Cast<InventorySlot>())
        {
            cell.ClearItem();
        }
    }

    public bool TryChangeCountsOrCancel(IEnumerable<(string, int)> items)
    {
        var slots = this.slotContainer.GetChildren().Cast<InventorySlot>().ToList();
        var before = slots.Select(a => a.GetItem()).ToList();

        foreach (var item in items)
        {
            var result = this.TryChangeCount(item.Item1, item.Item2);
            if (result != 0)
            {
                for (var i = 0; i < before.Count; i++)
                {
                    slots[i].ForceSetCount(before[i].Item1, before[i].Item2);
                }
                return false;
            }
        }

        return true;
    }

    public List<InventorySlot> GetItems()
    {
        return this.slotContainer.GetChildren().Cast<InventorySlot>().ToList();
    }

    #endregion
}

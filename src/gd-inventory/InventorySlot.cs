using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("InventorySlot.tscn")]
[Tool]
public partial class InventorySlot
{
    [Export]
    public int ItemIndex = -1;

    [Export]
    public List<Texture> Resources = new List<Texture>();

    private int itemsCount = 0;

    [Export]
    public int MaxCount;

    [Export]
    public int ItemsCount
    {
        get => itemsCount; set
        {
            itemsCount = value;
            if (IsInsideTree())
            {
                this.countLabel.Text = value.ToString();
                this.countLabel.Visible = value > 1;
            }
        }
    }

    [Signal]
    public delegate void UseItem();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.RemoveItem();

        this.ItemsCount = itemsCount;
    }

    public bool HasItem()
    {
        if ((this.lootContainer.GetChildCount() == 0 || ItemIndex == -1 || ItemsCount == 0) &&
            (this.lootContainer.GetChildCount() != 0 || ItemIndex != -1 || ItemsCount != 0))
        {
            throw new Exception("Inventory slot state inconsistent.");
        }

        return this.lootContainer.GetChildCount() > 0;
    }

    public int TryAddItem(int itemIndex, int countDiff)
    {
        if (HasItem() && this.ItemIndex != itemIndex)
        {
            GD.PrintErr("Can not add loot item to the slot as it is already occupied by different resouce.");
            return countDiff;
        }

        if (!HasItem() && countDiff < 0)
        {
            return countDiff;
        }

        var result = this.ItemsCount + countDiff;
        if (result <= 0)
        {
            RemoveItem();
            return result;
        }

        if (!HasItem())
        {
            this.lootContainer.AddChild(new TextureRect
            {
                Texture = Resources[itemIndex],
                MouseFilter = MouseFilterEnum.Ignore
            });
            this.ItemIndex = itemIndex;
        }

        this.ItemsCount = Math.Min(result, MaxCount);
        result -= this.ItemsCount;
        return result;
    }

    public (int, int) GetItem()
    {
        if (this.HasItem())
        {
            return (this.ItemIndex, ItemsCount);
        }
        else
        {
            return (-1, 0);
        }
    }

    public void RemoveItem()
    {
        if (!HasItem())
        {
            return;
        }

        this.lootContainer.ClearChildren();
        this.lootContainer.RemoveChild(this.lootContainer.GetChild(0));
        this.ItemsCount = 0;
        this.ItemIndex = -1;
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (!HasItem())
        {
            return;
        }

        if (!(@event is InputEventMouseButton mouse))
        {
            return;
        }

        if (mouse.Doubleclick)
        {
            this.EmitSignal(nameof(UseItem));
        }
    }

    public override object GetDragData(Vector2 position)
    {
        if (!this.HasItem())
        {
            return null;
        }

        var child = (Control)this.lootContainer.GetChild(0).Duplicate();
        this.SetDragPreview(child);
        return this;
    }

    public override bool CanDropData(Vector2 position, object data)
    {
        var ddata = (InventorySlot)data;
        return !HasItem() || this.ItemIndex == ddata.ItemIndex;
    }

    public override void DropData(Vector2 position, object data)
    {
        base.DropData(position, data);
        var ddata = (InventorySlot)data;
        if (ddata == this)
        {
            return;
        }

        var item = ddata.GetItem();
        ddata.RemoveItem();
        var diff = this.TryAddItem(item.Item1, item.Item2);
        ddata.TryAddItem(item.Item1, diff);
    }
}

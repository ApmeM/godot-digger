using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("InventorySlot.tscn")]
[Tool]
public partial class InventorySlot
{
    [Export]
    public int ItemId = -1;

    public Inventory.InventoryConfig Config = new Inventory.InventoryConfig();

    public readonly HashSet<int> AcceptedTypes = new HashSet<int>();

    [Export]
    public Texture ItemTypePlaceholderTexture
    {
        get => itemTypePlaceholderTexture;
        set
        {
            itemTypePlaceholderTexture = value;
            if (IsInsideTree())
            {
                this.slotTypePlaceholder.Texture = value;
            }
        }
    }

    private int itemsCount = 0;
    private Texture itemTypePlaceholderTexture;

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

    [Signal]
    public delegate void DragAndDropComplete(InventorySlot from, InventorySlot to);

    [Signal]
    public delegate void ItemCountChanged(int itemId, int from, int to);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.RemoveItem();

        this.ItemsCount = itemsCount;
        this.ItemTypePlaceholderTexture = this.itemTypePlaceholderTexture;
    }

    public bool HasItem()
    {
        if ((this.lootContainer.GetChildCount() == 0 || ItemId == -1 || ItemsCount == 0) &&
            (this.lootContainer.GetChildCount() != 0 || ItemId != -1 || ItemsCount != 0))
        {
            throw new Exception("Inventory slot state inconsistent.");
        }

        return this.lootContainer.GetChildCount() > 0;
    }

    public int TryAddItem(int itemId, int countDiff)
    {
        if (this.AcceptedTypes.Count > 0 && !this.AcceptedTypes.Contains(this.Config.SlotConfigs[itemId].ItemType))
        {
            GD.PrintErr($"This slot does not accept this item type. ItemTypes: {this.Config.SlotConfigs[itemId].ItemType}, AcceptedType: {string.Join(",", this.AcceptedTypes)}");
            return countDiff;
        }

        if (HasItem() && this.ItemId != itemId)
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
            this.slotTypePlaceholder.Visible = false;
            this.lootContainer.AddChild(new TextureRect
            {
                Texture = Config.SlotConfigs[itemId].Texture,
                MouseFilter = MouseFilterEnum.Ignore
            });
            this.ItemId = itemId;
        }

        var before = this.ItemsCount;
        this.ItemsCount = Math.Min(result, this.Config.SlotConfigs[itemId].MaxCount);
        result -= this.ItemsCount;

        this.EmitSignal(nameof(ItemCountChanged), itemId, before, this.ItemsCount);
        return result;
    }

    public (int, int) GetItem()
    {
        if (this.HasItem())
        {
            return (this.ItemId, ItemsCount);
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
        this.slotTypePlaceholder.Visible = true;
        this.lootContainer.ClearChildren();
        this.lootContainer.RemoveChild(this.lootContainer.GetChild(0));
        var before = this.ItemsCount;
        var itemId = this.ItemId;
        this.ItemsCount = 0;
        this.ItemId = -1;
        this.EmitSignal(nameof(ItemCountChanged), itemId, before, this.ItemsCount);
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
        if (HasItem())
        {
            if (Config.SlotConfigs[this.ItemId].MergeActions.ContainsKey(ddata.ItemId))
            {
                return true;
            }

            return this.ItemId == ddata.ItemId;
        }
        else
        {
            return this.AcceptedTypes.Count == 0 || this.AcceptedTypes.Contains(this.Config.SlotConfigs[ddata.ItemId].ItemType);
        }
    }

    public override void DropData(Vector2 position, object data)
    {
        base.DropData(position, data);
        var ddata = (InventorySlot)data;
        if (ddata == this)
        {
            return;
        }

        var ditem = ddata.GetItem();
        var item = this.GetItem();

        if (ItemId == ddata.ItemId || !this.HasItem())
        {
            ddata.RemoveItem();
            var diff = this.TryAddItem(ditem.Item1, ditem.Item2);
            ddata.TryAddItem(ditem.Item1, diff);
        }
        else
        {
            ddata.TryAddItem(ditem.Item1, -1);
            this.TryAddItem(item.Item1, -1);
            if (this.ItemsCount == 0)
            {
                var config = Config.SlotConfigs[item.Item1];
                var action = config.MergeActions[ditem.Item1];
                this.TryAddItem(action, 1);
            }
        }

        this.EmitSignal(nameof(DragAndDropComplete), ddata, this);
        ddata.EmitSignal(nameof(DragAndDropComplete), ddata, this);
    }
}

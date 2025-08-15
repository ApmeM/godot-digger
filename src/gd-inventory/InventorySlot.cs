using System;
using System.Collections.Generic;
using Godot;

[SceneReference("InventorySlot.tscn")]
[Tool]
public partial class InventorySlot
{
    [Flags]
    public enum DropOnAnotherItemTypeAction
    {
        Not_Allowed = 1,
        Switch_Items = 2,
        Emit_Signal = 4
    }

    [Export]
    public DropOnAnotherItemTypeAction DropOnAnotherItemType = DropOnAnotherItemTypeAction.Not_Allowed;

    public struct InventorySlotConfig
    {

        public Texture Texture;
        public int MaxCount;
        public int ItemType;
    }

    private int itemId = -1;

    [Export]
    public int ItemId
    {
        get => itemId;
        set
        {
            itemId = value;

            if (itemId == -1 && this.itemsCount != 0)
            {
                this.ItemsCount = 0;
            }

            if (this.IsInsideTree())
            {
                this.lootContainer.RemoveChildren();
                this.slotTypePlaceholder.Visible = itemId == -1;
                if (itemId != -1)
                {
                    this.lootContainer.AddChild(new TextureRect
                    {
                        Texture = Config[itemId].Texture,
                        MouseFilter = MouseFilterEnum.Ignore
                    });
                }
            }
        }
    }
    public Dictionary<int, InventorySlotConfig> Config = new Dictionary<int, InventorySlotConfig>();

    public readonly HashSet<int> AcceptedTypes = new HashSet<int>();

    [Export]
    public bool CanDragData = true;

    private Texture itemTypePlaceholderTexture;

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

    [Export]
    public int ItemsCount
    {
        get => itemsCount; set
        {
            itemsCount = value;
            if (itemsCount == 0 && this.itemId != -1)
            {
                this.ItemId = -1;
            }

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
    public delegate void DragOnAnotherItemType(InventorySlot from, InventorySlot to);

    [Signal]
    public delegate void ItemCountChanged(int itemId, int from, int to);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.ClearItem();

        this.ItemId = itemId;
        this.ItemsCount = itemsCount;
        this.ItemTypePlaceholderTexture = this.itemTypePlaceholderTexture;
    }

    public bool HasItem()
    {
        if ((this.lootContainer.GetChildCount() == 0 || ItemId == -1 || ItemsCount == 0) &&
            (this.lootContainer.GetChildCount() != 0 || ItemId != -1 || ItemsCount != 0))
        {
            throw new Exception($"Inventory slot state inconsistent: child count = {this.lootContainer.GetChildCount()}, ItemId = {ItemId}, ItemsCount = {ItemsCount}");
        }

        return this.lootContainer.GetChildCount() > 0;
    }

    public void ForceSetCount(int itemId, int count)
    {
        var oldItemsCount = this.ItemsCount;
        var oldItemId = this.ItemId;
        this.ItemsCount = count;
        this.ItemId = itemId;
        this.EmitSignal(nameof(ItemCountChanged), oldItemId, oldItemsCount, this.ItemsCount);
    }

    public int TryChangeCount(int itemId, int countDiff)
    {
        if (!Config.ContainsKey(itemId))
        {
            GD.PrintErr($"Resource with index {itemId} is not known for this inventory.");
            return countDiff;
        }

        if (this.AcceptedTypes.Count > 0 && !this.AcceptedTypes.Contains(this.Config[itemId].ItemType))
        {
            GD.PrintErr($"This slot does not accept this item type. ItemTypes: {this.Config[itemId].ItemType}, AcceptedType: {string.Join(",", this.AcceptedTypes)}");
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
            ClearItem();
            return result;
        }

        if (!HasItem())
        {
            this.slotTypePlaceholder.Visible = false;
            this.ItemId = itemId;
        }

        var before = this.ItemsCount;
        this.ItemsCount = Math.Min(result, this.Config[itemId].MaxCount);
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

    public void ClearItem()
    {
        if (!HasItem())
        {
            return;
        }

        this.ForceSetCount(-1, 0);
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
        if (!CanDragData)
        {
            return null;
        }

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
            return this.ItemId == ddata.ItemId || this.DropOnAnotherItemType != DropOnAnotherItemTypeAction.Not_Allowed;
        }
        else
        {
            return this.AcceptedTypes.Count == 0 || this.AcceptedTypes.Contains(this.Config[ddata.ItemId].ItemType);
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
            var diff = this.TryChangeCount(ditem.Item1, ditem.Item2);
            if (diff > 0)
            {
                ddata.ForceSetCount(ditem.Item1, diff);
            }
            else
            {
                ddata.ClearItem();
            }
        }
        else
        {
            switch (DropOnAnotherItemType)
            {
                case DropOnAnotherItemTypeAction.Not_Allowed:
                    GD.PrintErr($"BUG: dropping item on a slot with {DropOnAnotherItemType} should not happen.");
                    break;
                case DropOnAnotherItemTypeAction.Switch_Items:
                    ddata.ClearItem();
                    this.ClearItem();

                    if (
                        this.TryChangeCount(ditem.Item1, ditem.Item2) != 0 ||
                        ddata.TryChangeCount(item.Item1, item.Item2) != 0)
                    {
                        ddata.ForceSetCount(ditem.Item1, ditem.Item2);
                        this.ForceSetCount(item.Item1, item.Item2);
                    }
                    break;
                case DropOnAnotherItemTypeAction.Emit_Signal:
                    this.EmitSignal(nameof(DragOnAnotherItemType), ddata, this);
                    break;
            }
        }
    }
}

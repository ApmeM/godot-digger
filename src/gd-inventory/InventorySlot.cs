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

    [Signal]
    public delegate void SlotItemDoubleClicked(InventorySlot slot);

    [Signal]
    public delegate void SlotItemRightClicked(InventorySlot slot);

    [Signal]
    public delegate void DragOnAnotherItemType(InventorySlot from, InventorySlot to);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.ItemTypePlaceholderTexture = this.itemTypePlaceholderTexture;

        RefreshFromDump();
    }

    private void RefreshFromDump()
    {
        if (!Godot.Object.IsInstanceValid(this) || !this.IsInsideTree())
        {
            return;
        }

        this.countLabel.Text = this.ItemsCount.ToString();
        this.countLabel.Visible = this.ItemsCount > 1;

        this.lootContainer.RemoveChildren();
        this.slotTypePlaceholder.Visible = string.IsNullOrWhiteSpace(this.LootName);
        if (!this.slotTypePlaceholder.Visible)
        {
            var loot = LootDefinition.LootByName[this.LootName];
            this.lootContainer.AddChild(new TextureRect
            {
                Texture = loot.Image,
                MouseFilter = MouseFilterEnum.Ignore
            });
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (!this.HasItem())
        {
            return;
        }

        if (!(@event is InputEventMouseButton mouse))
        {
            return;
        }

        if (mouse.Doubleclick)
        {
            this.EmitSignal(nameof(SlotItemDoubleClicked), this);
            return;
        }

        if (mouse.Pressed)
        {
            switch ((ButtonList)mouse.ButtonIndex)
            {
                case ButtonList.Right:
                    this.EmitSignal(nameof(SlotItemRightClicked), this);
                    break;
            }
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
        var ditem = (InventorySlot)data;
        var item = this;

        if (item.HasItem())
        {
            return item.LootName == ditem.LootName || this.DropOnAnotherItemType != DropOnAnotherItemTypeAction.Not_Allowed;
        }
        else
        {
            var dLoot = ditem.LootDefinition;
            return item.AcceptedTypes.Count == 0 || item.AcceptedTypes.Contains(dLoot.ItemType);
        }
    }

    public override void DropData(Vector2 position, object data)
    {
        base.DropData(position, data);
        var ditem = (InventorySlot)data;
        var item = this;

        if (ditem == item)
        {
            return;
        }

        if (item.LootName == ditem.LootName || !item.HasItem())
        {
            var diff = item.TryChangeCount(ditem.LootName, ditem.ItemsCount);
            if (diff > 0)
            {
                ditem.ForceSetCount(ditem.LootName, diff);
            }
            else
            {
                ditem.ClearItem();
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
                    ditem.ClearItem();
                    item.ClearItem();

                    if (
                        item.TryChangeCount(ditem.LootName, ditem.ItemsCount) != 0 ||
                        ditem.TryChangeCount(item.LootName, item.ItemsCount) != 0)
                    {
                        ditem.ForceSetCount(ditem.LootName, ditem.ItemsCount);
                        item.ForceSetCount(item.LootName, item.ItemsCount);
                    }
                    break;
                case DropOnAnotherItemTypeAction.Emit_Signal:
                    this.EmitSignal(nameof(DragOnAnotherItemType), (InventorySlot)data, this);
                    break;
            }
        }
    }

    #region Data

    private string lootName;
    public string LootName => this.lootName;
    private int itemsCount;
    public int ItemsCount => this.itemsCount;

    public LootDefinition LootDefinition => string.IsNullOrWhiteSpace(this.lootName) ? null : LootDefinition.LootByName[this.lootName];

    public readonly HashSet<ItemType> AcceptedTypes = new HashSet<ItemType>();

    [Signal]
    public delegate void SlotContentChanged(InventorySlot slot);
    private void InternalSlotContentChanged()
    {
        this.EmitSignal(nameof(SlotContentChanged), this);
    }

    public bool HasItem()
    {
        return ItemsCount > 0;
    }

    public (string, int) GetItem()
    {
        return (this.lootName, this.itemsCount);
    }

    public void ClearItem()
    {
        this.ForceSetCount("", 0);
    }

    public void ForceSetCount(string lootName, int itemsCount)
    {
        if (string.IsNullOrWhiteSpace(LootName) && ItemsCount > 0)
        {
            throw new Exception($"Inventory slot state inconsistent: LootName = {LootName}, ItemsCount = {ItemsCount}");
        }

        this.itemsCount = itemsCount;
        this.lootName = lootName;
        this.RefreshFromDump();
        this.InternalSlotContentChanged();
    }

    public int TryChangeCount(string lootName, int countDiff)
    {
        var loot = LootDefinition.LootByName[lootName];

        if (this.AcceptedTypes.Count > 0 && !this.AcceptedTypes.Contains(loot.ItemType))
        {
            GD.PrintErr($"This slot does not accept this item type. ItemTypes: {loot.ItemType}, AcceptedType: {string.Join(",", this.AcceptedTypes)}");
            return countDiff;
        }

        if (HasItem() && this.LootName != lootName)
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

        var count = Math.Min(result, loot.MaxCount);
        result -= count;

        ForceSetCount(lootName, count);

        return result;
    }
    #endregion
}

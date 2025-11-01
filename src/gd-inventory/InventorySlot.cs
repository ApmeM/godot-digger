using System;
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

    public (string, int) Loot
    {
        get => (this.slotData.LootName, this.slotData.ItemsCount);
        set => this.slotData.ForceSetCount(value.Item1, value.Item2);
    }

    private InventorySlotData slotData = new InventorySlotData();
    public InventorySlotData SlotData
    {
        get => slotData;
        set
        {
            if (slotData == value)
            {
                return;
            }
            this.slotData.SlotContentChanged -= RefreshFromDump;
            slotData = value;
            this.slotData.SlotContentChanged += RefreshFromDump;
            RefreshFromDump();
        }
    }

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
    public delegate void UseItem();

    [Signal]
    public delegate void DragOnAnotherItemType(InventorySlot from, InventorySlot to);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.ItemTypePlaceholderTexture = this.itemTypePlaceholderTexture;

        this.slotData.SlotContentChanged += this.RefreshFromDump;
        RefreshFromDump();
    }

    private void RefreshFromDump()
    {
        if (!Godot.Object.IsInstanceValid(this) || !this.IsInsideTree())
        {
            return;
        }

        this.countLabel.Text = this.slotData.ItemsCount.ToString();
        this.countLabel.Visible = this.slotData.ItemsCount > 1;

        this.lootContainer.RemoveChildren();
        this.slotTypePlaceholder.Visible = string.IsNullOrWhiteSpace(this.slotData.LootName);
        if (!this.slotTypePlaceholder.Visible)
        {
            var loot = LootDefinition.LootByName[this.slotData.LootName];
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
        if (!this.slotData.HasItem())
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

        if (!this.slotData.HasItem())
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
        if (this.slotData.HasItem())
        {
            return this.slotData.LootName == ddata.slotData.LootName || this.DropOnAnotherItemType != DropOnAnotherItemTypeAction.Not_Allowed;
        }
        else
        {
            var dLoot = ddata.slotData.LootDefinition;
            return this.slotData.AcceptedTypes.Count == 0 || this.slotData.AcceptedTypes.Contains(dLoot.ItemType);
        }
    }

    public override void DropData(Vector2 position, object data)
    {
        base.DropData(position, data);
        var ditem = ((InventorySlot)data).slotData;
        var item = this.slotData;

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
}

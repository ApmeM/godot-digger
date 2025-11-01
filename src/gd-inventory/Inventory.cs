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
    public delegate void UseItem(InventorySlot slot);

    [Signal]
    public delegate void DragOnAnotherItemType(InventorySlot from, InventorySlot to);

    private InventoryData slotData = new InventoryData();
    public InventoryData SlotData
    {
        get => slotData;
        set
        {
            if (slotData == value)
            {
                return;
            }
            this.slotData.SlotsCountChanged -= RefreshFromDump;
            slotData = value;
            this.slotData.SlotsCountChanged += RefreshFromDump;
            RefreshFromDump();
        }
    }

    [Export]
    public uint Size
    {
        get => this.slotData.SlotsCount;
        set => this.slotData.SlotsCount = value;
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
        this.Title = this.title;
        this.DropOnAnotherItemType = this.dropOnAnotherItemType;

        this.slotData.SlotsCountChanged += this.RefreshFromDump;
        RefreshFromDump();
    }

    private void RefreshFromDump()
    {
        if (!Godot.Object.IsInstanceValid(this) || !this.IsInsideTree())
        {
            return;
        }

        // ToDo: check the problem with events.
        this.slotContainer.RemoveChildren();

        for (var i = 0; i < this.slotData.SlotsCount; i++)
        {
            var slot = this.InventorySlotScene.Instance<InventorySlot>();
            this.slotContainer.AddChild(slot);
            slot.Connect(nameof(InventorySlot.UseItem), this, nameof(SlotUseItem), new Godot.Collections.Array { slot });
            slot.Connect(nameof(InventorySlot.DragOnAnotherItemType), this, nameof(SlotDragOnAnotherItemType));
        }
    }
}

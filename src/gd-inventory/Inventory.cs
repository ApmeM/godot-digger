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

    private void SlotItemDoubleClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemDoubleClicked), slot);
    }
    private void SlotItemRightClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemRightClicked), slot);
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
            slot.SlotData = this.slotData.Slots[i];
            this.slotContainer.AddChild(slot);
            slot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
            slot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
            slot.Connect(nameof(InventorySlot.DragOnAnotherItemType), this, nameof(SlotDragOnAnotherItemType));
        }
    }
}

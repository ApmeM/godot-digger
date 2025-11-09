using Godot;

[SceneReference("EquipmentInventory.tscn")]
public partial class EquipmentInventory
{
    [Signal]
    public delegate void SlotItemDoubleClicked(InventorySlot slot);

    [Signal]
    public delegate void SlotItemRightClicked(InventorySlot slot);

    private EquipmentInventoryData slotData = new EquipmentInventoryData();
    public EquipmentInventoryData SlotData
    {
        get => slotData;
        set
        {
            if (slotData == value)
            {
                return;
            }
            slotData = value;
            RefreshFromDump();
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

    public override void _Ready()
    {
        RefreshFromDump();
        var all = new[]{
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

        foreach (var slot in all)
        {
            slot.Connect(nameof(InventorySlot.SlotItemDoubleClicked), this, nameof(SlotItemDoubleClickedHandler));
            slot.Connect(nameof(InventorySlot.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        }
    }

    public void RefreshFromDump()
    {
        this.neckSlot.SlotData = this.slotData.Neck;
        this.helmSlot.SlotData = this.slotData.Helm;
        this.weaponSlot.SlotData = this.slotData.Weapon;
        this.chestSlot.SlotData = this.slotData.Chest;
        this.shieldSlot.SlotData = this.slotData.Shield;
        this.ring1Slot.SlotData = this.slotData.Ring1;
        this.beltSlot.SlotData = this.slotData.Belt;
        this.ring2Slot.SlotData = this.slotData.Ring2;
        this.pantsSlot.SlotData = this.slotData.Pants;
        this.bootsSlot.SlotData = this.slotData.Boots;
    }
}

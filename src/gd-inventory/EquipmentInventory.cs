using Godot;

[SceneReference("EquipmentInventory.tscn")]
public partial class EquipmentInventory
{
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

    public override void _Ready()
    {
        RefreshFromDump();
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

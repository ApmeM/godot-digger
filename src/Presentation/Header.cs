using Godot;

[SceneReference("Header.tscn")]
[Tool]
public partial class Header
{
    public BagInventoryPopup BagInventoryPopup => this.bagInventoryPopup;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(OpenInventory));
        this.bagInventoryPopup.Connect(nameof(BagInventoryPopup.UseItem), this, nameof(InventoryUseItem));
    }

    protected async void InventoryUseItem(InventorySlot inventorySlot)
    {
        var slot = inventorySlot.SlotData;

        if (slot.LootDefinition?.UseAction != null)
        {
            var isUsed = await slot.LootDefinition.UseAction(this.GetParent<Game>());
            if (isUsed)
            {
                slot.TryChangeCount(slot.LootName, -1);
            }
        }
    }

    private BaseUnit trackingUnit;
    public BaseUnit TrackingUnit
    {
        get => this.trackingUnit;
        set
        {
            if (this.trackingUnit == value)
            {
                return;
            }

            this.trackingUnit = value;

            if (this.trackingUnit == null)
            {
                this.buffContainer.SlotData = new BuffsListData();
                this.bagInventoryPopup.SlotData = new BagInventoryData();
            }
            else
            {
                this.buffContainer.SlotData = this.trackingUnit.Buffs;
                this.bagInventoryPopup.SlotData = this.trackingUnit.Inventory;
            }
            
            UpdateTrackingUnit();
        }
    }

    public void UpdateTrackingUnit()
    {
        var character = this.trackingUnit;
        if (this.trackingUnit == null)
        {
            return;
        }

        this.inventoryBagItem.Visible = character.Inventory.Bag.HasItem();
        this.inventoryBagItem.Texture = character.Inventory.Bag.LootDefinition?.Image;

        this.staminaIconFront.Visible = character.Inventory.Equipment.Weapon.HasItem();
        this.staminaIconBack.Visible = !character.Inventory.Equipment.Weapon.HasItem();

        this.staminaIconFront.Texture = character.Inventory.Equipment.Weapon.LootDefinition?.Image;

        this.hpProgress.MaxValue = character.MaxHP;
        this.hpProgress.Value = character.HP;
        this.hpLabel.Text = character.HP.ToString();

        this.staminaLabel.Text = character.MaxStamina.ToString();
    }

    private void OpenInventory()
    {
        this.bagInventoryPopup.Show();
    }
}

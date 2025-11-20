using System;
using Godot;

[SceneReference("Header.tscn")]
[Tool]
public partial class Header
{
    [Signal]
    public delegate void SlotItemRightClicked(InventorySlot slot);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(OpenInventory));
    }

    protected async void InventoryUseItem(InventorySlot inventorySlot)
    {
        var slot = inventorySlot;

        if (slot.LootDefinition?.UseAction != null)
        {
            var isUsed = await slot.LootDefinition.UseAction?.Invoke(this.GetParent<Game>());
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

            if (this.trackingUnit != null)
            {
                this.trackingUnit.Buffs.Disconnect(nameof(BuffContainer.BuffClicked), this, nameof(BuffClickedHandler));
                this.buffPlaceholder.RemoveChild(this.trackingUnit.Buffs);
                this.trackingUnit.AddChild(this.trackingUnit.Buffs);
                this.trackingUnit.Buffs.Visible = false;
            }

            this.trackingUnit = value;

            if (this.trackingUnit != null)
            {
                this.trackingUnit.Buffs.Visible = true;
                this.trackingUnit.RemoveChild(this.trackingUnit.Buffs);
                this.buffPlaceholder.AddChild(this.trackingUnit.Buffs);
                this.trackingUnit.Buffs.Connect(nameof(BuffContainer.BuffClicked), this, nameof(BuffClickedHandler));
            }

            UpdateTrackingUnit();
        }
    }

    private void BuffClickedHandler(BaseBuff buff)
    {
        this.buffDescriptionLabel.Text = buff.Description;
        this.buffPopup.Show();
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

    private void CloseInventory()
    {
        this.GetTree().Paused = false;
        this.trackingUnit.Inventory.Disconnect(nameof(CustomPopup.PopupClosed), this, nameof(CloseInventory));
        this.trackingUnit.Inventory.Disconnect(nameof(BagInventoryPopup.SlotItemDoubleClicked), this, nameof(InventoryUseItem));
        this.trackingUnit.Inventory.Disconnect(nameof(BagInventoryPopup.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));

        this.RemoveChild(this.trackingUnit.Inventory);
        this.trackingUnit.AddChild(this.trackingUnit.Inventory);

    }

    private void OpenInventory()
    {
        if (this.trackingUnit == null)
        {
            return;
        }

        this.GetTree().Paused = true;
        this.trackingUnit.Inventory.Connect(nameof(CustomPopup.PopupClosed), this, nameof(CloseInventory));
        this.trackingUnit.Inventory.Connect(nameof(BagInventoryPopup.SlotItemDoubleClicked), this, nameof(InventoryUseItem));
        this.trackingUnit.Inventory.Connect(nameof(BagInventoryPopup.SlotItemRightClicked), this, nameof(SlotItemRightClickedHandler));
        this.trackingUnit.Inventory.Show();

        this.trackingUnit.RemoveChild(this.trackingUnit.Inventory);
        this.AddChild(this.trackingUnit.Inventory);
    }

    private void SlotItemRightClickedHandler(InventorySlot slot)
    {
        this.EmitSignal(nameof(SlotItemRightClicked), slot);
    }
}

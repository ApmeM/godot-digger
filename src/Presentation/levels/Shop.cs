using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Shop.tscn")]
public partial class Shop
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.shopInventory.Connect(nameof(Inventory.DragAndDropComplete), this, nameof(UpdateCost));
    }

    public override void InitMap(uint maxNumberOfTurns, uint inventorySlots, uint digPower)
    {
        base.InitMap(maxNumberOfTurns, inventorySlots, digPower);

        this.shopInventory.Resources = Resources;
        this.shopSellButton.Connect(CommonSignals.Pressed, this, nameof(SellButtonClicked));
    }

    private void SellButtonClicked()
    {
        var items = this.shopInventory.GetItems();
        foreach (var item in items)
        {
            var lootId = item.Item1;
            var tileId = this.MapLootIdToTileId[lootId];
            this.Money += LootDefinition.KnownLoot[(tileId, 0, 0)].Price * item.Item2;
        }
        this.shopInventory.ClearItems();
        UpdateCost(null);
    }

    private void UpdateCost(InventorySlot slot)
    {
        this.shopSellButton.Text = $"Sell for {CalculatePrice()}";
    }

    private int CalculatePrice()
    {
        var items = this.shopInventory.GetItems();
        var money = 0;
        foreach (var item in items)
        {
            var lootId = item.Item1;
            var tileId = this.MapLootIdToTileId[lootId];
            money += LootDefinition.KnownLoot[(tileId, 0, 0)].Price * item.Item2;
        }

        return money;
    }

    public override async void CustomBlockClicked(Vector2 pos)
    {
        if (pos == new Vector2(6, 10))
        {
            this.shopInventory.Visible = true;
            this.shopSellButton.Visible = true;
            this.bagInventoryPopup.Show();
            await this.ToSignal(this.bagInventoryPopup, nameof(CustomPopup.PopupClosed));
            this.shopInventory.Visible = false;
            this.shopSellButton.Visible = false;
            return;
        }

        base.CustomBlockClicked(pos);
    }
}

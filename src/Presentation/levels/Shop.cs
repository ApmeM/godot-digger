using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Shop.tscn")]
public partial class Shop
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        
        this.BagInventoryPopup.ConfigureInventory(this.shopInventory);

        this.shopInventory.Connect(nameof(Inventory.ItemCountChanged), this, nameof(UpdateCost));
        this.shopInventory.Config = this.BagInventoryPopup.Config;
        this.shopSellButton.Connect(CommonSignals.Pressed, this, nameof(SellButtonClicked));

        this.shopkeeper.Connect(CommonSignals.Pressed, this, nameof(ShopkeeperClicked));
    }

    private async void ShopkeeperClicked()
    {
            this.shopInventory.Visible = true;
            this.shopSellButton.Visible = true;
            this.BagInventoryPopup.CloseOnClickButton = false;
            this.BagInventoryPopup.Show();
            await this.ToSignal(this.BagInventoryPopup, nameof(CustomPopup.PopupClosed));
            this.shopInventory.Visible = false;
            this.shopSellButton.Visible = false;
            this.BagInventoryPopup.CloseOnClickButton = true;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        this.shopInventory.QueueFree();
    }

    private void SellButtonClicked()
    {
        var price = CalculatePrice();
        if (price == 0)
        {
            this.BagInventoryPopup.Close();
            return;
        }

        this.BagInventoryPopup.TryChangeCount(nameof(Gold), (int)price);
        this.shopInventory.ClearItems();
    }

    private void UpdateCost(InventorySlot slot, int itemId, int from, int to)
    {
        var price = CalculatePrice();
        if (price == 0)
            this.shopSellButton.Text = $"Close";
        else
            this.shopSellButton.Text = $"Sell for {CalculatePrice()}";
    }

    private long CalculatePrice()
    {
        var items = this.shopInventory.GetItems();
        var money = 0L;
        foreach (var item in items)
        {
            if (item.Item1 < 0)
            {
                continue;
            }
            var lootId = item.Item1;
            money += LootDefinition.LootById[lootId].Price * item.Item2;
        }

        return money;
    }

    // ToDo: buy loot
    // public async void CustomLootClickedAsync(Vector2 pos)
    // {
        // var tileId = this.loot.GetCellv(pos);
        // var coord = this.loot.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        // var price = LootDefinition.KnownLoot[(tileId, (int)coord.x, (int)coord.y)].Price;
        // var result = await questPopup.ShowQuestPopup("To buy:",
        //     new[] { (Loot.Gold, price) },
        //     new ValueTuple<ValueTuple<int, int, int>, uint>[] { });
        // if (!result)
        // {
        //     return;
        // }
    // }
}

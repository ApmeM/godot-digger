using System;
using System.Threading.Tasks;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Shop.tscn")]
public partial class Shop
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.shopInventory.Connect(nameof(Inventory.ItemCountChanged), this, nameof(UpdateCost));
        this.shopInventory.Config = Resources;
        this.shopSellButton.Connect(CommonSignals.Pressed, this, nameof(SellButtonClicked));
    }

    private void SellButtonClicked()
    {
        var price = CalculatePrice();
        if (price == 0)
        {
            this.bagInventoryPopup.Close();
            return;
        }

        this.bagInventory.TryChangeCount(Loot.Gold.Item1, (int)price);
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
            var tileId = item.Item1;
            money += LootDefinition.KnownLoot[(tileId, 0, 0)].Price * item.Item2;
        }

        return money;
    }

    public override async void CustomBlockClicked(Vector2 pos)
    {
        if (pos == new Vector2(5, 12))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level1");
            return;
        }
        if (pos == new Vector2(6, 10))
        {
            this.shopInventory.Visible = true;
            this.shopSellButton.Visible = true;
            this.bagInventoryPopup.CloseOnClickButton = false;
            this.bagInventoryPopup.Show();
            await this.ToSignal(this.bagInventoryPopup, nameof(CustomPopup.PopupClosed));
            this.shopInventory.Visible = false;
            this.shopSellButton.Visible = false;
            this.bagInventoryPopup.CloseOnClickButton = true;
            return;
        }

        base.CustomBlockClicked(pos);
    }

    public override async void CustomLootClickedAsync(Vector2 pos)
    {
        var tileId = this.loot.GetCellv(pos);
        var coord = this.loot.GetCellAutotileCoord((int)pos.x, (int)pos.y);

        var price = LootDefinition.KnownLoot[(tileId, (int)coord.x, (int)coord.y)].Price;
        var result = await ShowQuestPopup("To buy:",
            new[] { (Loot.Gold, price) },
            new ValueTuple<ValueTuple<int, int, int>, uint>[] { });
        if (!result)
        {
            return;
        }

        base.CustomLootClickedAsync(pos);
    }
}

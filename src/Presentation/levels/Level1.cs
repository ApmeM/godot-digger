using Godot;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.stashInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
        foreach (var slot in Resources)
        {
            this.stashInventory.Config.Add(slot.Key, new InventorySlot.InventorySlotConfig
            {
                MaxCount = slot.Value.MaxCount * 10,
                Texture = slot.Value.Texture,
            });
        }
    }

    public override async void CustomConstructionClickedAsync(Vector2 pos)
    {
        if (pos == new Vector2(4, 12))
        {
            signLabel.Text = "Welcome to our glorious city (Currently abandoned).\nTutorial: wood pile requires to click twice to clean.";
            signPopup.Show();
            return;
        }
        if (pos == new Vector2(0, 18))
        {
            signLabel.Text = "Tutorial: Click on the grass above to open the road.";
            signPopup.Show();
            return;
        }
        if (pos == new Vector2(17, 1))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level2");
            return;
        }
        if (pos == new Vector2(10, 3))
        {
            this.EmitSignal(nameof(ChangeLevel), "Woodcutter");
            return;
        }
        if (pos == new Vector2(4, 2))
        {
            this.EmitSignal(nameof(ChangeLevel), "Shop");
            return;
        }
        if (pos == new Vector2(1, 4))
        {
            this.stashInventory.Visible = true;
            this.bagInventoryPopup.Show();
            await this.ToSignal(this.bagInventoryPopup, nameof(CustomPopup.PopupClosed));
            this.stashInventory.Visible = false;
            return;
        }
        if (pos == new Vector2(24, 17))
        {
            var result = await ShowQuestPopup("Did you bring me a bread from my daughter RedHat?",
                new[] { (Loot.Bread, 1u) },
                new[] { (Loot.Gold, 1u) }
            );
            if (result)
            {
                signLabel.Text = "Thank you young man.";
                signPopup.Show();
            }
            return;
        }

        base.CustomConstructionClickedAsync(pos);
    }

    public override async void CustomBlockClicked(Vector2 pos)
    {
        if (pos == new Vector2(4, 2))
        {
            var result = await ShowQuestPopup("Hi stranger, I'm a shop keeper without shop.\nCan you please bring me wood \nand I'll build a shop with useful items for you.\n\nTutorial: wood can be found in \n wood piles or in a forest.",
                new[] { (Loot.Wood, 1u) },
                new[] { (Loot.Gold, 1u) }
            );
            if (result)
            {
                signLabel.Text = "Thanks.";
                signPopup.Show();
                this.blocks.SetCellv(pos, -1);
                this.constructions.SetCellv(pos, Constructions.Blacksmith.Item1, autotileCoord: new Vector2(Constructions.Blacksmith.Item2, Constructions.Blacksmith.Item3));
            }
            return;
        }
        if (pos == new Vector2(9, 13))
        {
            var result = await ShowQuestPopup("Hi strong man, I afraid to go to my grandma through the forrest. There are a lot of wolfs. For each wolf skin I'll gibe you a bread that is very tasty (doubleclick on it from the inventory).",
                new[] { (Loot.WolfSkin, 1u) },
                new[] { (Loot.Bread, 1u) }
            );
            if (result)
            {
                signLabel.Text = "Thanks. Take your bread.";
                signPopup.Show();
            }
            return;
        }
        base.CustomBlockClicked(pos);
    }
}

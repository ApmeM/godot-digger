using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.stashInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
    }

    public override void InitMap(uint maxNumberOfTurns, uint inventorySlots, uint digPower)
    {
        base.InitMap(maxNumberOfTurns, inventorySlots, digPower);

        this.stashInventory.Resources = Resources;
    }

    public override void ShowPopup(Vector2 pos)
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
        base.ShowPopup(pos);
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
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
        base.ChangeLevelClicked(pos);
    }

    public override async void CustomConstructionClicked(Vector2 pos)
    {
        if (pos == new Vector2(1, 4))
        {
            this.stashInventory.Visible = true;
            this.bagInventoryPopup.Show();
            await this.ToSignal(this.bagInventoryPopup, nameof(CustomPopup.PopupClosed));
            this.stashInventory.Visible = false;
            return;
        }
        if (pos == new Vector2(4, 2))
        {
            var result = await ShowQuestPopup("Do you want to forge a better instrument?",new List<Tuple<ValueTuple<int, int, int>, uint>>{
                new Tuple<ValueTuple<int,int,int>, uint>(Loot.Steel, Fibonacci.Calc(this.DigPower + 5))
            });
            if (result)
            {
                this.DigPower++;
            }
            return;
        }
        if (pos == new Vector2(24, 17))
        {
            var result = await ShowQuestPopup("Did you bring me a bread from my daughter RedHat?", new List<Tuple<ValueTuple<int, int, int>, uint>>{
                new Tuple<ValueTuple<int,int,int>, uint>(Loot.Bread, 1)
            });
            if (result)
            {
                this.DigPower++;
            }
            return;
        }

        base.CustomConstructionClicked(pos);
    }

    public override async void CustomBlockClicked(Vector2 pos)
    {
        if (pos == new Vector2(4, 2))
        {
            var result = await ShowQuestPopup("Hi stranger, I'm a shop keeper without shop.\nCan you please bring me wood \nand I'll build a shop with useful items for you.\n\nTutorial: wood can be found in \n wood piles or in a forest.", new List<Tuple<ValueTuple<int, int, int>, uint>>{
                new Tuple<ValueTuple<int,int,int>, uint>(Loot.Wood, 1)
            });
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
            var result = await ShowQuestPopup("Hi strong man, I afraid to go to my grandma through the forrest. There are a lot of wolfs. For each wolf skin I'll gibe you a bread that is very tasty (doubleclick on it from the inventory).", new List<Tuple<ValueTuple<int, int, int>, uint>>{
                new Tuple<ValueTuple<int,int,int>, uint>(Loot.WolfSkin, 1)
            });
            if (result)
            {
                signLabel.Text = "Thanks. Take your bread.";
                var lootId = this.MapTileIdToLootId[Loot.Bread.Item1];
                this.bagInventory.TryAddItem(lootId, 1);
            }
            return;
        }
        base.CustomBlockClicked(pos);
    }
}

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
        if (pos == new Vector2(16, 11))
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
            this.stashInventoryPopup.Show();
            return;
        }
        if (pos == new Vector2(4, 2))
        {
            var result = await ShowQuestPopup("Do you want to forge a better instrument?", new List<Tuple<int, uint>>{
                new Tuple<int, uint>(1, Fibonacci.Calc(this.DigPower + 5))
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
            var result = await ShowQuestPopup("Hi stranger, I'm a shop keeper without shop.\nCan you please bring me wood \nand I'll build a shop with useful items for you.\n\nTutorial: wood can be found in \n wood piles or in a forest.", new List<Tuple<int, uint>>{
                new Tuple<int, uint>(0, 1)
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
        base.CustomBlockClicked(pos);
    }
}

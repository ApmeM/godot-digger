using System.Collections.Generic;
using Godot;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.BagInventoryPopup.ConfigureInventory(this.stashInventory);

        foreach (var slot in this.BagInventoryPopup.Config)
        {
            this.stashInventory.Config.Add(slot.Key, new InventorySlot.InventorySlotConfig
            {
                MaxCount = slot.Value.MaxCount * 10,
                Texture = slot.Value.Texture,
            });
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        this.stashInventory.QueueFree();
    }

    public override async void CustomBlockClicked(Vector2 pos)
    {
        if (pos == new Vector2(4, 12))
        {
            this.ShowPopup("Welcome to our glorious city (Currently abandoned).\nTutorial: wood pile requires to click twice to clean.");
            return;
        }
        if (pos == new Vector2(0, 18))
        {
            this.ShowPopup("Tutorial: Click on the grass above to open the road.");
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
        if (pos == new Vector2(1, 4))
        {
            this.stashInventory.Visible = true;
            this.BagInventoryPopup.Show();
            await this.ToSignal(this.BagInventoryPopup, nameof(CustomPopup.PopupClosed));
            this.stashInventory.Visible = false;
            return;
        }
        if (pos == new Vector2(24, 17))
        {
            var result = await questPopup.ShowQuestPopup("Did you bring me a bread from my daughter RedHat?",
                new[] { (Loot.Bread, 1u) },
                new[] { (Loot.Gold, 1u) }
            );
            if (result)
            {
                this.ShowPopup("Thank you young man.");
            }
            return;
        }
        if (pos == new Vector2(4, 2))
        {
            if (this.blocks.GetCellv(pos) == Blocks.Shopkeeper.Item1)
            {
                var result = await questPopup.ShowQuestPopup("Hi stranger, I'm a shop keeper without shop.\nCan you please bring me wood \nand I'll build a shop with useful items for you.\n\nTutorial: wood can be found in \n wood piles or in a forest.",
                    new[] { (Loot.Wood, 1u) },
                    new[] { (Loot.Gold, 1u) }
                );
                if (result)
                {
                    this.ShowPopup("Thanks.");
                    this.blocks.SetCellv(pos, Blocks.BlacksmithHouse.Item1, autotileCoord: new Vector2(Blocks.BlacksmithHouse.Item2, Blocks.BlacksmithHouse.Item3));
                }
                return;
            }
            else
            {
                this.EmitSignal(nameof(ChangeLevel), "Shop");
            }
        }
        if (pos == new Vector2(9, 13))
        {
            var result = await questPopup.ShowQuestPopup("Hi strong man, I afraid to go to my grandma through the forrest. There are a lot of wolfs. For each wolf skin I'll gibe you a bread that is very tasty (doubleclick on it from the inventory).",
                new[] { (Loot.WolfSkin, 1u) },
                new[] { (Loot.Bread, 1u) }
            );
            if (result)
            {
                this.ShowPopup("Thanks. Take your bread.");
            }
            return;
        }
        base.CustomBlockClicked(pos);
    }

    public class CustomData
    {
        public List<(int, int)> Stash;
    }

    public override LevelDump GetLevelDump()
    {
        var dump = base.GetLevelDump();
        dump.CustomData = new CustomData
        {
            Stash = this.stashInventory.GetItems()
        };
        return dump;
    }

    public override void LoadLevelDump(LevelDump levelDump)
    {
        base.LoadLevelDump(levelDump);

        if (levelDump == null)
        {
            return;
        }

        var customData = (CustomData)levelDump.CustomData;
        if (customData != null)
        {
            this.stashInventory.SetItems(customData.Stash);
        }
        else
        {
            this.stashInventory.ClearItems();
        }
    }
}

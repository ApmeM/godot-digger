using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Main.tscn")]
public partial class Main
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        this.gamePosition.Visible = false;
        this.menuPosition.Visible = true;

        this.achievements.Connect(CommonSignals.Pressed, this, nameof(AchievementsPressed));
        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(ShowInventoryPopup));
        var number = LootTexture.GetWidth() / 16;
        for (var i = 0; i < number; i++)
        {
            var lootItem = new AtlasTexture
            {
                Atlas = LootTexture,
                Region = new Rect2(i * 16, 0, 16, 16)
            };

            this.inventory.Resources.Add(lootItem);
        }

        this.buildingBlacksmith.Initialize(() => new List<Tuple<Loot, uint>> { new Tuple<Loot, uint>(Loot.Steel, Fibonacci.Calc(this.DigPower + 5)) }, () => this.DigPower++);
        this.buildingLeather.Initialize(() => new List<Tuple<Loot, uint>> { new Tuple<Loot, uint>(Loot.Cloth, Fibonacci.Calc(this.InventorySlots)) }, () => this.InventorySlots++);

        foreach (var child in this.GetTree().GetNodesInGroup(Groups.LevelButton))
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            level.Connect(CommonSignals.Pressed, this, nameof(LevelPressed), new Godot.Collections.Array { level.GameToStart });
        }
    }

    public void ExitDungeon(int stairsType, string fromLevel, List<Loot> resources)
    {
        this.gamePosition.ClearChildren();
        this.ResourcesAdded(resources);

        if ((Floor)stairsType == Floor.StairsUp)
        {
            this.gamePosition.Visible = false;
            this.menuPosition.Visible = true;
            return;
        }
        var nextLevel = this.GetNextLevel(stairsType, fromLevel);

        this.OpenLevel(nextLevel);
        this.LoadLevel(nextLevel);
    }

    [Export]
    public Texture LootTexture;

    [Export]
    public uint InventorySlots = 3;

    [Export]
    public uint DigPower = 1;

    [Export]
    public uint MaxNumberOfTurns = 10;

    private void ShowInventoryPopup()
    {
        this.customPopupInventory.Show();
    }

    public void ResourcesAdded(List<Loot> newResources)
    {
        foreach (var res in newResources)
        {
            this.inventory.TryAddItem((int)res, 1);
        }
    }

    private void AchievementsPressed()
    {
        this.achievementList.ReloadList();
        this.customPopupAchievements.Show();
    }

    private void LevelPressed(PackedScene levelScene)
    {
        this.gamePosition.Visible = true;
        this.menuPosition.Visible = false;
        var game = levelScene.Instance<BaseLevel>();
        game.Resources = this.inventory.Resources;
        game.InitMap(this.MaxNumberOfTurns, this.InventorySlots, this.DigPower);
        game.Connect(nameof(BaseLevel.ExitDungeon), this, nameof(ExitDungeon));
        this.gamePosition.AddChild(game);
    }

    public void LoadLevel(string levelName)
    {
        foreach (var child in this.GetTree().GetNodesInGroup(Groups.LevelButton))
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            if (level.LevelName == levelName)
            {
                LevelPressed(level.GameToStart);
                return;
            }
        }
    }

    public void OpenLevel(string levelName)
    {
        foreach (var child in this.GetTree().GetNodesInGroup(Groups.LevelButton))
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            if (level.LevelName == levelName)
            {
                level.Visible = true;
                return;
            }
        }
    }

    public string GetNextLevel(int stairsType, string fromLevel)
    {
        foreach (var child in this.GetTree().GetNodesInGroup(Groups.LevelButton))
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            if (level.LevelName == fromLevel)
            {
                return level.NextLevelButton.IsEmpty() ? null : level.GetNextLevel().LevelName;
            }
        }

        return null;
    }
}

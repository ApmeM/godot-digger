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
        this.blacksmith.Connect(CommonSignals.Pressed, this, nameof(BlacksmithPressed));
        this.leather.Connect(CommonSignals.Pressed, this, nameof(LeatherPressed));
        this.inventory.Connect(CommonSignals.Pressed, this, nameof(ShowInventoryPopup));
        var number = LootTexture.GetWidth() / 16;
        for (var i = 0; i < number; i++)
        {
            this.customPopupInventory.Resources.Add(new AtlasTexture
            {
                Atlas = LootTexture,
                Region = new Rect2(i * 16, 0, 16, 16)
            });
        }

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
        this.customPopupInventory.ShowCentered();
    }

    public void ResourcesAdded(List<Loot> newResources)
    {
        foreach (var res in newResources)
        {
            this.customPopupInventory.TryAddItem((int)res, 1);
        }
    }

    private void AchievementsPressed()
    {
        this.achievementList.ReloadList();
        this.customPopupAchievements.ShowCentered();
    }

    private void LevelPressed(PackedScene levelScene)
    {
        this.gamePosition.Visible = true;
        this.menuPosition.Visible = false;
        var game = levelScene.Instance<BaseLevel>();
        game.LootTexture = this.LootTexture;
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

    private void BlacksmithPressed()
    {
        SpendResource(Loot.Steel, "irons", Fibonacci.Calc(this.DigPower + 5), () => this.DigPower++);
    }

    private void LeatherPressed()
    {
        SpendResource(Loot.Cloth, "cloth", Fibonacci.Calc(this.InventorySlots), () => this.InventorySlots++);
    }

    private async void SpendResource(Loot res, string resName, uint required, Action action)
    {
        var existing = this.customPopupInventory.GetItemCount((int)res);
        if (existing >= required)
        {
            this.customConfirmPopup.ContentText = $"Increase pickaxe power?\nIt requires {required} {resName}.";
            this.customConfirmPopup.ShowCentered();
            var decision = (bool)(await ToSignal(this.customConfirmPopup, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
            if (decision)
            {
                existing = this.customPopupInventory.GetItemCount((int)res);
                if (existing >= required)
                {
                    this.customPopupInventory.TryRemoveItems((int)res, required);
                    action();
                }
            }
        }
        else
        {
            this.customTextPopup.ContentText = $"Not enough {resName}.\n{required} {resName} required.";
            this.customTextPopup.ShowCentered();
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

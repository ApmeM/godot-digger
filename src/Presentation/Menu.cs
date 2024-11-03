using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Menu.tscn")]
public partial class Menu
{
    [Signal]
    public delegate void LevelSelected(PackedScene levelScene);
    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.ResourcesChanged), this, nameof(ResourcesChanged));
        this.gameState.Connect(nameof(GameState.LoadLevel), this, nameof(LoadLevel));
        this.gameState.Connect(nameof(GameState.OpenedLevelsChanged), this, nameof(OpenedLevelsChanged));
        this.achievements.Connect(CommonSignals.Pressed, this, nameof(AchievementsPressed));
        this.dungeon.Connect(CommonSignals.Pressed, this, nameof(DungeonPressed));
        this.sleep.Connect(CommonSignals.Pressed, this, nameof(SleepPressed));
        this.blacksmith.Connect(CommonSignals.Pressed, this, nameof(BlacksmithPressed));
        this.exit.Connect(CommonSignals.Pressed, this, nameof(ExitPressed));
        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            level.Connect(CommonSignals.Pressed, this, nameof(LevelPressed), new Godot.Collections.Array { level.dungeonScene });
        }
    }

    private void OpenedLevelsChanged()
    {
        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            level.Disabled = !this.gameState.IsLevelOpened(level.LevelName);
        }

        // First level alvays visible
        this.level1.Disabled = false;
    }

    private void ResourcesChanged()
    {
        this.woodCount.Text = this.gameState.GetResource(Loot.Wood).ToString();
        this.steelCount.Text = this.gameState.GetResource(Loot.Steel).ToString();
    }

    private void AchievementsPressed()
    {
        this.achievementList.ReloadList();
        this.windowDialog.PopupCentered();
    }

    private void DungeonPressed()
    {
        this.levelSelector.Visible = false;
        this.dungeonSelector.Visible = true;
    }

    private void ExitPressed()
    {
        this.levelSelector.Visible = true;
        this.dungeonSelector.Visible = false;
    }

    private void LevelPressed(PackedScene levelScene)
    {
        this.DungeonPressed();
        this.EmitSignal(nameof(LevelSelected), levelScene);
    }

    public void LoadLevel(string levelName)
    {
        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            if (level.LevelName == levelName)
            {
                LevelPressed(level.dungeonScene);
                return;
            }
        }
    }

    private void SleepPressed()
    {
        this.gameState.NumberOfTurns += 5;
    }

    private void BlacksmithPressed()
    {
        var irons = this.gameState.GetResource(Loot.Steel);
        if (irons >= 5 * this.gameState.DigPower)
        {
            this.gameState.DigPower++;
            this.gameState.AddResource(Loot.Steel, -5);
        }
    }

    public string GetNextLevel(int stairsType, string fromLevel)
    {
        switch (fromLevel)
        {
            case "Level1":
                return "Level2";
            default:
                throw new Exception("Victory!!!");
        }
    }
}

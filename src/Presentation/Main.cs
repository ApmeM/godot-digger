using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Levels;
using GodotDigger.LevelSelector;
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

        this.game.Visible = false;

        this.achievementsButton.Connect(CommonSignals.Pressed, this, nameof(AchievementsButtonPressed));
        this.levelSelector.SetLevels(new List<ILevelToSelect> { new Dungeon() });
        this.levelSelector.Connect(nameof(LevelSelector.StartGame), this, nameof(LevelSelected));

        this.gameState.NumberOfTurns = 10;
    }

    private void LevelSelected(int gameId)
    {
        this.game.Visible = true;
        this.levelSelector.Visible = false;
        this.menuLayer.Visible = false;
        this.levelSelector.GetLevel(gameId).Init(this.game);
    }

    private void AchievementsButtonPressed()
    {
        this.levelSelector.Visible = !this.levelSelector.Visible;
        this.achievementList.Visible = !this.achievementList.Visible;

        // See achievements definitions in gd-achievements/achievements.json
        this.achievementList.ReloadList();
    }

    public void ExitDungeon()
    {
        this.game.Visible = false;
        this.levelSelector.Visible = true;
        this.menuLayer.Visible = true;
    }
}

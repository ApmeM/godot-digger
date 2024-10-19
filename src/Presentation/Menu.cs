using Godot;
using GodotDigger.Levels;
using GodotDigger.LevelSelector;
using GodotDigger.Presentation.Utils;
using System;
using System.Collections.Generic;


[SceneReference("Menu.tscn")]
public partial class Menu
{
    [Signal]
    public delegate void LevelSelected(int levelId);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.achievementsButton.Connect(CommonSignals.Pressed, this, nameof(AchievementsButtonPressed));
        this.levelSelector.SetLevels(new List<ILevelToSelect> { new Dungeon() });
        this.levelSelector.Connect(nameof(LevelSelector.StartGame), this, nameof(LevelSelectorSelected));
    }

    private void AchievementsButtonPressed()
    {
        this.levelSelector.Visible = !this.levelSelector.Visible;
        this.achievementList.Visible = !this.achievementList.Visible;

        // See achievements definitions in gd-achievements/achievements.json
        this.achievementList.ReloadList();
    }


    private void LevelSelectorSelected(int gameId)
    {
        this.EmitSignal(nameof(LevelSelected), gameId);
    }
}

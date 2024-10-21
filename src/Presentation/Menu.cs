using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Menu.tscn")]
public partial class Menu
{
    [Signal]
    public delegate void LevelSelected(int levelId);
    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.NumberOfTurnsChanged), this, nameof(NumberOfTurnsChanged));
        this.gameState.Connect(nameof(GameState.ResourcesChanged), this, nameof(ResourcesChanged));
        this.achievements.Connect(CommonSignals.Pressed, this, nameof(AchievementsPressed));
        this.dungeon.Connect(CommonSignals.Pressed, this, nameof(DungeonPressed), new Godot.Collections.Array { 0 });
        this.sleep.Connect(CommonSignals.Pressed, this, nameof(SleepPressed));
        this.blacksmith.Connect(CommonSignals.Pressed, this, nameof(BlacksmithPressed));
    }

    private void NumberOfTurnsChanged()
    {
        this.turnsCount.Text = this.gameState.NumberOfTurns.ToString();
    }

    private void ResourcesChanged()
    {
        this.woodCount.Text = this.gameState.GetResource(Resources.Iron).ToString();
    }

    private void AchievementsPressed()
    {
        this.achievementList.ReloadList();
        this.windowDialog.PopupCentered();

        // See achievements definitions in gd-achievements/achievements.json
    }

    private void DungeonPressed(int gameId)
    {
        this.EmitSignal(nameof(LevelSelected), gameId);
    }

    private void SleepPressed()
    {
        this.gameState.NumberOfTurns += 5;
    }

    private void BlacksmithPressed()
    {
        var irons = this.gameState.GetResource(Resources.Iron);
        if (irons >= 5 * this.gameState.DigPower)
        {
            this.gameState.DigPower++;
            this.gameState.AddResource(Resources.Iron, -5);
        }
    }
}

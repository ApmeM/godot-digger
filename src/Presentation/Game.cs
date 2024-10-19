using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Game.tscn")]
public partial class Game
{
    public int WoodCountValue
    {
        get => int.Parse(this.woodCount.Text);
        set => this.woodCount.Text = value.ToString();
    }

    [Signal]
    public delegate void ExitDungeon();

    [Export]
    public PackedScene Level1Scene;
    public BaseLevel map;
    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.NumberOfTurnsChanged), this, nameof(NumberOfTurnsChanged));

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));
    }

    private void NumberOfTurnsChanged()
    {
        this.turnsCount.Text = this.gameState.NumberOfTurns.ToString();
    }

    private void VisibilityChanged()
    {
        this.header.Visible = this.Visible;
        this.mapHolder.Visible = this.Visible;
    }
    
    public void InitMap()
    {
        this.map = this.Level1Scene.Instance<Level1>();
        this.mapHolder.AddChild(this.map);
        this.map.Connect(nameof(BaseLevel.ActionableCellClicked), this, nameof(ActionableCellClicked));
        this.map.Connect(nameof(BaseLevel.ExitCellClicked), this, nameof(ExitCellClicked));
    }

    private void ExitCellClicked()
    {
        this.mapHolder.ClearChildren();
        this.EmitSignal(nameof(ExitDungeon));
    }

    public void ActionableCellClicked(Vector2 cell)
    {
        if (this.gameState.NumberOfTurns > 0)
        {
            this.gameState.NumberOfTurns--;
            this.map.ActOnCell(cell);
        }
    }
}

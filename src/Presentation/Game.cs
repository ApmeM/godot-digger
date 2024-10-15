using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Game.tscn")]
public partial class Game
{
    public int TurnsCountValue
    {
        get => int.Parse(this.turnsCount.Text);
        set => this.turnsCount.Text = value.ToString();
    }

    public int WoodCountValue
    {
        get => int.Parse(this.woodCount.Text);
        set => this.woodCount.Text = value.ToString();
    }

    [Export]
    public PackedScene Level1Scene;
    public BaseLevel map;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));
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
        this.GetParent<Main>().ExitDungeon();;
    }

    public void ActionableCellClicked(Vector2 cell)
    {
        if (this.TurnsCountValue > 0)
        {
            this.TurnsCountValue--;
            this.map.ActOnCell(cell);
        }
    }
}

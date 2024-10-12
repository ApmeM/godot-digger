using System;
using Godot;
using GodotAnalysers;
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

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));

        this.fog.Visible = true;
        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                if (this.map.GetCellv(new Vector2(x, y)) >= 0)
                {
                    this.fog.SetCellv(new Vector2(x, y), 0, autotileCoord: new Vector2(0, 0));
                }
            }
        }

        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                var cell = new Vector2(x, y);
                if (this.map.GetCellv(cell) == 0 && this.map.GetCellAutotileCoord(x, y) == new Vector2(7, 0))
                {
                    this.fog.SetCellv(new Vector2(x, y), -1);
                    this.fog.SetCellv(new Vector2(x - 1, y), -1);
                    this.fog.SetCellv(new Vector2(x + 1, y), -1);
                    this.fog.SetCellv(new Vector2(x, y - 1), -1);
                    this.fog.SetCellv(new Vector2(x, y + 1), -1);
                }
            }
        }

        this.TurnsCountValue = 10;
    }

    private void VisibilityChanged()
    {
        this.map.Visible = this.Visible;
        this.fog.Visible = this.Visible;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventScreenTouch eventMouseButton && eventMouseButton.Pressed)
        {
            var position = map.ToLocal(eventMouseButton.Position);

            var cell = map.WorldToMap(position);

            if (this.fog.GetCellv(cell) != -1)
            {
                // Clicked on an unknown cell.
                return;
            }

            if (TurnsCountValue > 0)
                if (this.map.GetCellAutotileCoord((int)cell.x, (int)cell.y) == new Vector2(13, 0))
                {
                    // Clicked on a cell that can be removed.
                    this.map.SetCellv(cell, 0, autotileCoord: new Vector2(1, 0));
                    this.TurnsCountValue--;
                    this.WoodCountValue++;

                    this.fog.SetCellv(new Vector2(cell.x - 1, cell.y), -1);
                    this.fog.SetCellv(new Vector2(cell.x + 1, cell.y), -1);
                    this.fog.SetCellv(new Vector2(cell.x, cell.y - 1), -1);
                    this.fog.SetCellv(new Vector2(cell.x, cell.y + 1), -1);
                }

            // Clicked on a cell that have nothing.
        }
    }
}

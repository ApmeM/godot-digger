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
/*
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
            {
                if (this.CurrentMap[(int)cell.x, (int)cell.y].Clickable)
                {
                    this.CurrentMap[(int)cell.x, (int)cell.y].HP--;
                    if (this.CurrentMap[(int)cell.x, (int)cell.y].HP == 0)
                    {
                        this.CurrentMap[(int)cell.x, (int)cell.y] = CellDefinition.KnownCells[CellDefinition.Path];

                        // Clicked on a cell that can be removed.
                        this.map.SetCellv(cell, 0, autotileCoord: CellDefinition.Path);
                        this.TurnsCountValue--;
                        this.WoodCountValue++;

                        this.fog.SetCellv(new Vector2(cell.x - 1, cell.y), -1);
                        this.fog.SetCellv(new Vector2(cell.x + 1, cell.y), -1);
                        this.fog.SetCellv(new Vector2(cell.x, cell.y - 1), -1);
                        this.fog.SetCellv(new Vector2(cell.x, cell.y + 1), -1);
                    }
                }
            }
            // Clicked on a cell that have nothing.
        }
    }
*/

    public void InitMap()
    {
        this.map = this.Level1Scene.Instance<Level1>();
        this.mapHolder.AddChild(this.map);
    }
}

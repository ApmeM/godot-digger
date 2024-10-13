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

    [Export]
    public PackedScene Level1Scene;
    public Level1 map;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));

        this.map = this.Level1Scene.Instance<Level1>();
        this.mapHolder.AddChild(this.map);
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

    private CellDefinition[,] CurrentMap = new CellDefinition[10, 16];

    public void InitMap()
    {
        this.FixVisibility();
        this.CopyLevel(this.map);
        this.HideMapWithFog();
    }

    private void FixVisibility()
    {
        this.fog.Visible = true;
        this.map.Visible = true;
    }

    private void CopyLevel(TileMap level)
    {
        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                var cell = new Vector2(x, y);
                this.map.SetCellv(cell, level.GetCellv(cell), autotileCoord: level.GetCellAutotileCoord(x, y));
                var set = level.GetCellv(cell);
                if (set >= 0)
                {
                    var tile = level.GetCellAutotileCoord(x, y);
                    if (CellDefinition.KnownCells.ContainsKey(tile))
                    {
                        this.CurrentMap[x, y] = CellDefinition.KnownCells[tile].Clone();
                    }
                    else
                    {
                        GD.PrintErr($"Unkonwn cell: {tile}");
                    }
                }
            }
        }
    }

    private void HideMapWithFog()
    {
        for (var x = 0; x < 9; x++)
        {
            for (var y = 0; y < 15; y++)
            {
                if (this.map.GetCellv(new Vector2(x, y)) >= 0)
                {
                    this.fog.SetCellv(new Vector2(x, y), 0, autotileCoord: new Vector2(0, 0));
                }
                else
                {
                    this.fog.SetCellv(new Vector2(x, y), -1);
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
    }
}

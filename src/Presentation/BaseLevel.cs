using System.Collections.Generic;
using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    private Dictionary<Vector2, CellDefinition> CurrentMap = new Dictionary<Vector2, CellDefinition>();

    [Signal]
    public delegate void ActionableCellClicked(Vector2 cell);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.FillCurrentMap();
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

            var cellTile = this.map.GetCellAutotileCoord((int)cell.x, (int)cell.y);
            if (cellTile == CellDefinition.Path || cellTile == CellDefinition.Wall || cellTile == CellDefinition.Stairs)
            {
                //Clicked on an unactionable cell.
                return;
            }

            this.EmitSignal(nameof(ActionableCellClicked), cell);
        }
    }

    public void ActOnCell(Vector2 cell)
    {
        if (!this.CurrentMap.ContainsKey(cell))
        {
            GD.PrintErr($"Cell {cell} not exists in CurrentMap.");
            return;
        }

        this.CurrentMap[cell].HP--;
        if (this.CurrentMap[cell].HP == 0)
        {
            this.CurrentMap[cell] = CellDefinition.KnownCells[CellDefinition.Path];

            // Clicked on a cell that can be removed.
            this.map.SetCellv(cell, 0, autotileCoord: CellDefinition.Path);
            this.UnFogCell(cell);
        }
    }

    public void FillCurrentMap()
    {
        foreach (Vector2 cell in this.map.GetUsedCells())
        {
            var set = this.map.GetCellv(cell);
            var tile = this.map.GetCellAutotileCoord((int)cell.x, (int)cell.y);

            this.fog.SetCell((int)cell.x, (int)cell.y, 0, autotileCoord: Vector2.Zero);

            if (!CellDefinition.KnownCells.ContainsKey(tile))
            {
                GD.PrintErr($"Unkonwn cell: {tile}");
                return;
            }

            this.CurrentMap[cell] = CellDefinition.KnownCells[tile].Clone();
        }

        foreach (Vector2 cell in this.map.GetUsedCells())
        {
            var set = this.map.GetCellv(cell);
            var tile = this.map.GetCellAutotileCoord((int)cell.x, (int)cell.y);

            if (tile == CellDefinition.Stairs)
            {
                UnFogCell(cell);
            }
        }
    }

    private void UnFogCell(Vector2 cell)
    {
        this.fog.SetCellv(cell, -1);
        this.fog.SetCellv(new Vector2(cell.x - 1, cell.y), -1);
        this.fog.SetCellv(new Vector2(cell.x + 1, cell.y), -1);
        this.fog.SetCellv(new Vector2(cell.x, cell.y - 1), -1);
        this.fog.SetCellv(new Vector2(cell.x, cell.y + 1), -1);
    }
}

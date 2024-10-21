using System.Collections.Generic;
using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    private Dictionary<Vector2, CellDefinition> CurrentMap = new Dictionary<Vector2, CellDefinition>();
    private GameState gameState;

    [Signal]
    public delegate void ActionableCellClicked(Vector2 cell);

    [Signal]
    public delegate void ExitCellClicked();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.FillCurrentMap();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventScreenTouch eventMouseButton && eventMouseButton.Pressed)
        {
            var position = this.floor.ToLocal(eventMouseButton.Position);

            var cell = this.floor.WorldToMap(position);

            if (this.fog.GetCellv(cell) != -1)
            {
                // Clicked on an unknown cell.
                return;
            }

            var cellTile = this.items.GetCellAutotileCoord((int)cell.x, (int)cell.y);
            if (cellTile == CellDefinition.Stairs)
            {
                this.EmitSignal(nameof(ExitCellClicked));
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

        this.CurrentMap[cell].HP -= this.gameState.DigPower;
        if (this.CurrentMap[cell].HP <= 0)
        {
            var cellData = this.CurrentMap[cell];
            // Clicked on a cell that can be removed.

            this.CurrentMap.Remove(cell);
            this.items.SetCellv(cell, -1);
            this.UnFogCell(cell);

            this.gameState.AddResource(cellData.Resource, cellData.ResourceCount);
        }
    }

    public void FillCurrentMap()
    {
        foreach (Vector2 cell in this.floor.GetUsedCells())
        {
            this.fog.SetCell((int)cell.x, (int)cell.y, 0, autotileCoord: Vector2.Zero);
        }

        foreach (Vector2 cell in this.items.GetUsedCells())
        {
            var set = this.items.GetCellv(cell);
            var tile = this.items.GetCellAutotileCoord((int)cell.x, (int)cell.y);

            if (!CellDefinition.KnownCells.ContainsKey(tile))
            {
                GD.PrintErr($"Unkonwn cell: {tile}");
                continue;
            }

            this.CurrentMap[cell] = CellDefinition.KnownCells[tile].Clone();
        }

        foreach (Vector2 cell in this.items.GetUsedCells())
        {
            var set = this.items.GetCellv(cell);
            var tile = this.items.GetCellAutotileCoord((int)cell.x, (int)cell.y);

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

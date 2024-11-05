using System.Collections.Generic;
using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    private Dictionary<Vector2, CellDefinition> CurrentMap = new Dictionary<Vector2, CellDefinition>();
    private GameState gameState;

    [Signal]
    public delegate void ExitCellClicked(int stairsType);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.FillCurrentMap();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");

        this.AddToGroup(Groups.SavedScene);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventScreenTouch eventMouseButton && !eventMouseButton.Pressed)
        {
            var pos = this.floor.WorldToMap(this.floor.ToLocal(eventMouseButton.Position));

            var fogCell = this.fog.GetCellv(pos);
            var fogCellTile = this.fog.GetCellAutotileCoord((int)pos.x, (int)pos.y);
            if (fogCell != -1)
            {
                // Clicked on an unknown cell.
                return;
            }

            var blocksCell = this.blocks.GetCellv(pos);
            var blocksCellTile = this.blocks.GetCellAutotileCoord((int)pos.x, (int)pos.y);
            var lootCell = this.loot.GetCellv(pos);
            var lootCellTile = this.loot.GetCellAutotileCoord((int)pos.x, (int)pos.y);
            if (blocksCell >= 0 &&
                ((Blocks)blocksCellTile.x == Blocks.StairsUp || (Blocks)blocksCellTile.x == Blocks.StairsDown))
            {
                this.EmitSignal(nameof(ExitCellClicked), (int)blocksCellTile.x);
                return;
            }

            if (blocksCell != -1)
            {
                if (this.gameState.NumberOfTurns > 0)
                {
                    this.gameState.NumberOfTurns--;
                    if (!this.CurrentMap.ContainsKey(pos))
                    {
                        GD.PrintErr($"Cell at {pos} not exists in CurrentMap.");
                        return;
                    }

                    if (this.CurrentMap[pos].HP > this.gameState.DigPower)
                    {
                        this.CurrentMap[pos].HP -= this.gameState.DigPower;
                        return;
                    }

                    var cellData = this.CurrentMap[pos];
                    this.CurrentMap.Remove(pos);
                    this.blocks.SetCellv(pos, -1);
                    this.UnFogCell(pos);
                }

                return;
            }

            if (lootCell != -1)
            {
                if (this.GetParent().GetParent<Game>().TryAddResource((Loot)lootCellTile.x, 1))
                {
                    this.loot.SetCellv(pos, -1);
                }
            }
        }
    }

    public void FillCurrentMap()
    {
        foreach (Vector2 cell in this.floor.GetUsedCells())
        {
            this.fog.SetCell((int)cell.x, (int)cell.y, 0, autotileCoord: Vector2.Zero);
        }

        foreach (Vector2 cell in this.blocks.GetUsedCells())
        {
            var blocksCell = this.blocks.GetCellv(cell);
            var blocksCellTile = this.blocks.GetCellAutotileCoord((int)cell.x, (int)cell.y);

            if (!CellDefinition.KnownBlocks.ContainsKey((Blocks)blocksCellTile.x))
            {
                GD.PrintErr($"Unkonwn cell: {blocksCellTile}");
                continue;
            }

            this.CurrentMap[cell] = CellDefinition.KnownBlocks[(Blocks)blocksCellTile.x].Clone();
        }

        foreach (Vector2 cell in this.blocks.GetUsedCells())
        {
            var set = this.blocks.GetCellv(cell);
            var tile = this.blocks.GetCellAutotileCoord((int)cell.x, (int)cell.y);

            if ((Blocks)tile.x == Blocks.StairsUp)
            {
                UnFogCell(cell);
            }
        }
    }

    private Vector2[] unfogDirections = new[]{
        Vector2.Left,
        Vector2.Right,
        Vector2.Up,
        Vector2.Down,
    };

    private void UnFogCell(Vector2 cell)
    {
        this.fog.SetCellv(cell, -1);
        foreach (var dir in unfogDirections)
        {
            var dirCell = cell + dir;
            if (this.fog.GetCellv(dirCell) == -1)
            {
                continue;
            }

            this.fog.SetCellv(dirCell, -1);
            if (this.blocks.GetCellv(dirCell) == -1 && this.floor.GetCellAutotileCoord((int)dirCell.x, (int)dirCell.y).x != 1)
            {
                UnFogCell(dirCell);
            }
        }
    }

    public void SaveFog(List<(Vector2, uint, Vector2)> list) => SaveTileMap(this.fog, list);
    public void SaveLoot(List<(Vector2, uint, Vector2)> list) => SaveTileMap(this.loot, list);
    public void SaveBlocks(List<(Vector2, uint, Vector2)> list) => SaveTileMap(this.blocks, list);
    public void SaveFloor(List<(Vector2, uint, Vector2)> list) => SaveTileMap(this.floor, list);
    private void SaveTileMap(TileMap level, List<(Vector2, uint, Vector2)> list)
    {
        list.Clear();
        var levelCells = level.GetUsedCells();
        foreach (Vector2 cell in levelCells)
        {
            list.Add((cell, (uint)level.GetCellv(cell), level.GetCellAutotileCoord((int)cell.x, (int)cell.y)));
        }
    }

    public void LoadFog(List<(Vector2, uint, Vector2)> list) => LoadTileMap(this.fog, list);
    public void LoadLoot(List<(Vector2, uint, Vector2)> list) => LoadTileMap(this.loot, list);
    public void LoadBlocks(List<(Vector2, uint, Vector2)> list) => LoadTileMap(this.blocks, list);
    public void LoadFloor(List<(Vector2, uint, Vector2)> list) => LoadTileMap(this.floor, list);
    private void LoadTileMap(TileMap level, List<(Vector2, uint, Vector2)> list)
    {
        level.Clear();
        foreach (var data in list)
        {
            level.SetCellv(data.Item1, (int)data.Item2, autotileCoord: data.Item3);
        }
    }

}

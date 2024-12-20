using System;
using System.Collections.Generic;
using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    private Dictionary<Vector2, CellDefinition> CurrentMap = new Dictionary<Vector2, CellDefinition>();

    [Signal]
    public delegate void ExitCellClicked(int stairsType);

    [Signal]
    public delegate void DigCellClicked();

    [Signal]
    public delegate void ResourceClicked(Loot resource, Vector2 atPosition);

    [Export]
    public bool CanDig = true;

    [Export]
    public uint DigPower = 1;


    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.FillCurrentMap();

        this.AddToGroup(Groups.LevelScene);
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
                if (CanDig)
                {
                    if (!this.CurrentMap.ContainsKey(pos))
                    {
                        GD.PrintErr($"Cell at {pos} not exists in CurrentMap.");
                        return;
                    }

                    this.EmitSignal(nameof(DigCellClicked));

                    if (this.CurrentMap[pos].HP > this.DigPower)
                    {
                        this.CurrentMap[pos].HP -= this.DigPower;
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
                this.EmitSignal(nameof(ResourceClicked), (Loot)lootCellTile.x, pos);
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

    public void RemoveResource(Vector2 atPosition)
    {
         this.loot.SetCellv(atPosition, -1);
    }
}

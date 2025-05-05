using System;
using System.Collections.Generic;
using System.Linq;
using BrainAI.Pathfinding;
using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel : IUnweightedGraph<Vector2>
{
    [Signal]
    public delegate void ChangeLevel(string nextLevel);

    public readonly Vector2[] cardinalDirections = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right };

    public Header HeaderControl;
    public BagInventoryPopup BagInventoryPopup;

    public TileMap FloorMap => this.floor;
    public FloatingTextManager FloatingTextManagerControl => this.floatingTextManager;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.draggableCamera.LimitLeft = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.x) * this.floor.CellSize.x * this.floor.Scale.x);
        this.draggableCamera.LimitRight = (int)Math.Max(this.GetViewport().Size.x, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.x + 1) * this.floor.CellSize.x * this.floor.Scale.x);
        this.draggableCamera.LimitTop = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.y) * this.floor.CellSize.y * this.floor.Scale.x);
        this.draggableCamera.LimitBottom = (int)Math.Max(this.GetViewport().Size.y, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.y + 1) * this.floor.CellSize.y * this.floor.Scale.x);

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        foreach (Vector2 cell in this.floor.GetUsedCells())
        {
            if ((Fog)this.fog.GetCellv(cell) == Fog.Nothing)
            {
                this.fog.SetCellv(cell, (int)Fog.Basic);
            }
        }

        foreach (BaseLoot baseLoot in this.GetTree().GetNodesInGroup(Groups.Loot))
        {
            baseLoot.LevelPath = this.GetPath();
        }

        foreach (BaseUnit baseUnit in this.GetTree().GetNodesInGroup(Groups.Unit))
        {
            baseUnit.LevelPath = this.GetPath();
        }

        ReFogMap();

        this.AddToGroup(Groups.LevelScene);
    }

    public void ReFogMap()
    {
        GD.Print("ReFogMap executed.");
        foreach (Vector2 cell in this.fog.GetUsedCellsById((int)Fog.NoFog))
        {
            this.fog.SetCellv(cell, (int)Fog.Basic);
        }

        foreach (Vector2 pos in this.fog.GetUsedCellsById((int)Fog.UnfogStart))
        {
            foreach (var dir in cardinalDirections)
            {
                UnFogCell(pos + dir);
            }
        }
    }

    protected void UnFogCell(Vector2 cell)
    {
        var queue = new Queue<Vector2>();
        queue.Enqueue(cell);

        while (queue.Any())
        {
            cell = queue.Dequeue();

            var fog = (Fog)this.fog.GetCellv(cell);

            if (fog == Fog.Nothing || fog == Fog.NoFog || fog == Fog.UnfogStart)
            {
                continue;
            }

            this.fog.SetCellv(cell, (int)Fog.NoFog);

            // ToDo: UnFogCell Take blocks into account
            // if (this.blocks.GetCellv(cell) != -1 && !this.Meta[cell].NoFogBlocker)
            // {
            //     continue;
            // }

            foreach (var dir in cardinalDirections)
            {
                queue.Enqueue(cell + dir);
            }
        }
    }

    public void GetNeighbors(Vector2 node, ICollection<Vector2> result)
    {
        if (/* this.blocks.GetCellv(node - Vector2.Down) == -1 &&  */this.floor.GetCellv(node - Vector2.Down) != -1) result.Add(node - Vector2.Down);
        if (/* this.blocks.GetCellv(node - Vector2.Left) == -1 &&  */this.floor.GetCellv(node - Vector2.Left) != -1) result.Add(node - Vector2.Left);
        if (/* this.blocks.GetCellv(node - Vector2.Right) == -1 &&  */this.floor.GetCellv(node - Vector2.Right) != -1) result.Add(node - Vector2.Right);
        if (/* this.blocks.GetCellv(node - Vector2.Up) == -1 &&  */this.floor.GetCellv(node - Vector2.Up) != -1) result.Add(node - Vector2.Up);
    }

    public virtual LevelDump GetLevelDump()
    {
        return new LevelDump
        {
            Floor = this.floor.GetUsedCells().Cast<Vector2>().Select(a => (a, this.floor.GetCellv(a))).ToList(),
            Fog = this.fog.GetUsedCells().Cast<Vector2>().Select(a => (a, this.fog.GetCellv(a))).ToList(),
            CameraZoom = this.draggableCamera.Zoom,
            CameraPos = this.draggableCamera.Position
        };
    }

    public virtual void LoadLevelDump(LevelDump levelDump)
    {
        if (levelDump == null)
        {
            return;
        }

        this.floor.Clear();
        this.fog.Clear();

        levelDump.Floor?.ForEach(a => this.floor.SetCellv(a.Item1, a.Item2));
        levelDump.Fog?.ForEach(a => this.fog.SetCellv(a.Item1, a.Item2));
        this.draggableCamera.Position = levelDump.CameraPos;
        this.draggableCamera.Zoom = levelDump.CameraZoom;
    }
}

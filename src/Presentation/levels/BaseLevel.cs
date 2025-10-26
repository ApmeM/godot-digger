using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainAI.Pathfinding;
using Godot;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel : IWeightedGraph<ValueTuple<Vector2, HashSet<Floor>>>
{
    [Signal]
    public delegate void ChangeLevel(string nextLevel);

    [Signal]
    public delegate void GameOver();

    public readonly Vector2[] moveDirections = new Vector2[] {
        Vector2.Down,
        Vector2.Down + Vector2.Left,
        Vector2.Left,
        Vector2.Left + Vector2.Up,
        Vector2.Up,
        Vector2.Up + Vector2.Right,
        Vector2.Right,
        Vector2.Right + Vector2.Down,
    };

    public Header HeaderControl;
    public BagInventoryPopup BagInventoryPopup;

    public FloatingTextManager FloatingTextManagerControl => this.floatingTextManager;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.draggableCamera.LimitLeft = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.x) * this.floor.CellSize.x * this.floor.Scale.x);
        // this.draggableCamera.LimitRight = (int)Math.Max(this.GetViewport().Size.x, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.x + 1) * this.floor.CellSize.x * this.floor.Scale.x);
        // this.draggableCamera.LimitTop = (int)Math.Min(0, this.floor.GetUsedCells().Cast<Vector2>().Min(a => a.y) * this.floor.CellSize.y * this.floor.Scale.x);
        // this.draggableCamera.LimitBottom = (int)Math.Max(this.GetViewport().Size.y, this.floor.GetUsedCells().Cast<Vector2>().Max(a => a.y + 1) * this.floor.CellSize.y * this.floor.Scale.x);

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        foreach (BaseUnit baseUnit in this.GetTree().GetNodesInGroup(Groups.Unit))
        {
            baseUnit.LevelPath = this.GetPath();
        }

        this.AddToGroup(Groups.LevelScene);
    }

    public void GetNeighbors((Vector2, HashSet<Floor>) node, ICollection<(Vector2, HashSet<Floor>)> result)
    {
        foreach (var dir in moveDirections)
        {
            if (/* this.blocks.GetCellv(node - dir) == -1 &&  */node.Item2.Contains((Floor)this.floor.GetCellv(node.Item1 - dir)))
            {
                result.Add((node.Item1 - dir, node.Item2));
            }
        }
    }

    public int Cost((Vector2, HashSet<Floor>) from, (Vector2, HashSet<Floor>) to)
    {
        return (int)(from.Item1 - to.Item1).Length();
    }

    public bool IsReachable(Vector2 pos, HashSet<Floor> floors)
    {
        return floors.Contains((Floor)this.floor.GetCellv(pos));
    }

    public Vector2 MapToWorld(Vector2 mapPos)
    {
        return this.floor.MapToWorld(mapPos) + this.floor.CellSize / 2;
    }

    public Vector2 WorldToMap(Vector2 worldPos)
    {
        return this.floor.WorldToMap(worldPos - this.floor.CellSize / 2);
    }

    public virtual LevelDump GetLevelDump()
    {
        return new LevelDump
        {
            Floor = this.floor.GetUsedCells().Cast<Vector2>().Select(a => (a, this.floor.GetCellv(a))).ToList(),
            // CameraZoom = this.draggableCamera.Zoom,
            // CameraPos = this.draggableCamera.Position
        };
    }

    public virtual void LoadLevelDump(LevelDump levelDump)
    {
        if (levelDump == null)
        {
            return;
        }

        this.floor.Clear();

        levelDump.Floor?.ForEach(a => this.floor.SetCellv(a.Item1, a.Item2));
        // this.draggableCamera.Position = levelDump.CameraPos;
        // this.draggableCamera.Zoom = levelDump.CameraZoom;
    }

    [Signal]
    public delegate void CellClicked(Vector2 cell, bool success);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed)
            {
                if ((ButtonList)mouseEvent.ButtonIndex == ButtonList.Left)
                {
                    var floorMousePos = floor.GetLocalMousePosition();
                    this.EmitSignal(nameof(CellClicked), floorMousePos, true);
                }
                if ((ButtonList)mouseEvent.ButtonIndex == ButtonList.Right)
                {
                    var floorMousePos = floor.GetLocalMousePosition();
                    this.EmitSignal(nameof(CellClicked), Vector2.Zero, false);
                }
            }
        }
    }

    internal void AddUnit(Vector2 position, string unitName)
    {
        var unit = Instantiator.CreateUnit(unitName);
        unit.Position = position;
        unit.LevelPath = this.GetPath();
        floor.AddChild(unit);
    }

    internal async Task<Vector2?> ChoosePosition()
    {
        this.BagInventoryPopup.Close();
        this.selectPosition.Show();
        var res = await this.ToMySignal<Vector2?, bool>(nameof(CellClicked));
        this.selectPosition.Hide();
        this.BagInventoryPopup.Show();
        return res.Item2 ? res.Item1 : null;
    }
}

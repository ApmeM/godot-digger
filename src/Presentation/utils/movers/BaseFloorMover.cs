using System.Collections.Generic;
using System.Linq;
using BrainAI.Pathfinding;
using Godot;

public abstract class BaseFloorMover : BaseMover
{
    public BaseFloorMover(BaseLevel level)
    {
        this.level = level;
    }

    private List<(Vector2, HashSet<Floor>)> moveResultPath = new List<(Vector2, HashSet<Floor>)>();

    private IPathfinder<(Vector2, HashSet<Floor>)> internalMovePathfinder;
    protected readonly BaseLevel level;

    private IPathfinder<(Vector2, HashSet<Floor>)> movePathfinder
    {
        get
        {
            internalMovePathfinder = internalMovePathfinder ?? new WeightedPathfinder<(Vector2, HashSet<Floor>)>(level);
            return internalMovePathfinder;
        }
    }

    public override Vector2? MoveUnit(BaseUnit unit)
    {
        if (unit.MoveFloors == null || unit.MoveFloors.Count == 0)
        {
            GD.PrintErr($"Automatic move enabled. Should move but have no floors defined : {unit.GetType()} {unit.GetPath()}");
            return null;
        }

        var maxDistance = unit.VisionDistance;
        var floors = unit.MoveFloorsSet;
        var targets = this.GetTargets(unit, floors, maxDistance);
        if (targets == null)
        {
            return null;
        }

        var pos = level.WorldToMap(unit.Position);
        moveResultPath.Clear();
        movePathfinder.Search((pos, floors), targets, (maxDistance * 2 + 1) * (maxDistance * 2 + 1), moveResultPath);

        if (moveResultPath == null || moveResultPath.Count < 2)
        {
            return null;
        }

        return moveResultPath[1].Item1;
    }

    public abstract HashSet<(Vector2, HashSet<Floor>)> GetTargets(BaseUnit unit, HashSet<Floor> floors, int maxDistance);
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainAI.Pathfinding;
using Godot;

public abstract class BaseFloorMover : BaseMover
{
    private List<(Vector2, HashSet<Floor>)> moveResultPath = new List<(Vector2, HashSet<Floor>)>();

    private IPathfinder<(Vector2, HashSet<Floor>)> internalMovePathfinder;

    private IPathfinder<(Vector2, HashSet<Floor>)> movePathfinder
    {
        get
        {
            internalMovePathfinder = internalMovePathfinder ?? new WeightedPathfinder<(Vector2, HashSet<Floor>)>(level);
            return internalMovePathfinder;
        }
    }

    protected BaseFloorMover(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override async Task<bool> MoveUnit()
    {
        if (unit.MoveFloors == null || unit.MoveFloors.Count == 0)
        {
            GD.PrintErr($"Automatic move enabled. Should move but have no floors defined : {unit.GetType()} {unit.GetPath()}");
            return false;
        }

        var maxDistance = unit.VisionDistance;
        var floors = unit.MoveFloorsSet;
        var targets = this.GetTargets(unit, floors, maxDistance);
        if (targets == null)
        {
            return false;
        }

        var pos = level.WorldToMap(unit.Position);
        moveResultPath.Clear();
        movePathfinder.Search((pos, floors), targets, (maxDistance * 2 + 1) * (maxDistance * 2 + 1), moveResultPath);

        if (moveResultPath == null || moveResultPath.Count < 2)
        {
            return false;
        }

        var moveNextStep = moveResultPath[1].Item1;
        var moveNextPosition = level.MapToWorld(moveNextStep);
        await unit.StartMoveAction(moveNextPosition);

        return true;
    }

    public abstract HashSet<(Vector2, HashSet<Floor>)> GetTargets(BaseUnit unit, HashSet<Floor> floors, int maxDistance);
}

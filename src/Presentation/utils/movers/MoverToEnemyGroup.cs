using System.Collections.Generic;
using System.Linq;
using Godot;

public class MoverToEnemyGroup : BaseFloorMover
{
    public MoverToEnemyGroup(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override HashSet<(Vector2, HashSet<Floor>)> GetTargets(BaseUnit unit, HashSet<Floor> floors, int maxDistance)
    {
        if (unit.AggroAgainst == null || unit.AggroAgainst.Count == 0)
        {
            return null;
        }

        var pos = level.WorldToMap(unit.Position);
        return unit.AggroAgainst
            .Except(unit.GetGroups().Cast<string>())
            .SelectMany(a => unit.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Select(a => (level.WorldToMap(a.Position), floors))
            .Where(a => level.IsReachable(a.Item1, floors))
            .Where(a => (a.Item1 - pos).LengthSquared() <= maxDistance * maxDistance)
            .ToHashSet();
    }
}
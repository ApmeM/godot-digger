using System.Collections.Generic;
using System.Linq;
using Godot;

public class MoverToLoot : BaseFloorMover
{
    public MoverToLoot(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override HashSet<(Vector2, HashSet<Floor>)> GetTargets(BaseUnit unit, HashSet<Floor> floors, int maxDistance)
    {
        if (!unit.GrabLoot)
        {
            return null;
        }

        var pos = level.WorldToMap(unit.Position);

        return unit.GetTree()
            .GetNodesInGroup(Groups.Loot)
            .Cast<BaseLoot>()
            .Select(a => (level.WorldToMap(a.Position), floors))
            .Where(a => level.IsReachable(a.Item1, floors))
            .Where(a => (a.Item1 - pos).LengthSquared() <= maxDistance * maxDistance)
            .ToHashSet();
    }
}
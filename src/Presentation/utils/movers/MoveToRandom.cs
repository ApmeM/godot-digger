using System;
using System.Collections.Generic;
using Godot;

public class MoveToRandom : BaseFloorMover
{
    private Random random = new Random();

    public MoveToRandom(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override HashSet<(Vector2, HashSet<Floor>)> GetTargets(BaseUnit unit, HashSet<Floor> floors, int maxDistance)
    {
        var pos = level.WorldToMap(unit.Position);
        var dest = pos + level.moveDirections[random.Next(level.moveDirections.Length)];
        if (!level.IsReachable(dest, floors))
        {
            return null;
        }
        return new HashSet<(Vector2, HashSet<Floor>)>
        {
            (dest, floors)
        };
    }
}
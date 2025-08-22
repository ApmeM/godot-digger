using System.Linq;

public class GrabLootMove : BaseMover
{
    public GrabLootMove(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override bool MoveUnit()
    {
        if (unit.GrabLoot)
        {
            var loot = unit.GetTree()
                .GetNodesInGroup(Groups.Loot)
                .Cast<BaseLoot>()
                .Where(a => level.WorldToMap(a.Position) == level.WorldToMap(unit.Position))
                .FirstOrDefault();

            if (loot != null)
            {
                unit.StartGrabLoot(loot);
                return true;
            }
        }
        return false;
    }
}
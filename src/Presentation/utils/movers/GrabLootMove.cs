using System.Linq;
using System.Threading.Tasks;

public class GrabLootMove : BaseMover
{
    public GrabLootMove(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override async Task<bool> MoveUnit()
    {
        if (!unit.GrabLoot)
        {
            return false;
        }

        var loot = unit.GetTree()
            .GetNodesInGroup(Groups.Loot)
            .Cast<BaseLoot>()
            .Where(a => level.WorldToMap(a.Position) == level.WorldToMap(unit.Position))
            .FirstOrDefault();

        if (loot == null)
        {
            return false;
        }

        await unit.StartGrabLoot(loot);
        return true;
    }
}
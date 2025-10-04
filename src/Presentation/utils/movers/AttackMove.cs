using System.Linq;
using System.Threading.Tasks;

public class AttackMove : BaseMover
{
    public AttackMove(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override async Task<bool> MoveUnit()
    {
        if (unit.AttackPower <= 0 || unit.AggroAgainst == null || unit.AggroAgainst.Count <= 0)
        {
            return false;
        }

        // AutoAttack nearest opponent.
        var opponent = unit.AggroAgainst
            .Except(unit.GetGroups().Cast<string>())
            .SelectMany(a => unit.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Where(a => (a.Position - unit.Position).LengthSquared() < unit.AttackDistance * unit.AttackDistance)
            .OrderBy(a => (a.Position - unit.Position).LengthSquared())
            .FirstOrDefault();
        if (opponent == null)
        {
            return false;
        }

        await unit.StartAttackAction(opponent);
        return true;
    }
}

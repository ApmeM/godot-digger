using System.Threading.Tasks;

public class StandStil : BaseMover
{
    public StandStil(BaseUnit unit, BaseLevel level) : base(unit, level)
    {
    }

    public override async Task<bool> MoveUnit()
    {
        await unit.StartStayAction();
        return true;
    }
}
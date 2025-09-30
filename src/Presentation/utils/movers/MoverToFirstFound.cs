using BrainAI.AI;

public class MoverToFirstFound : BaseMover, IAITurn
{
    private readonly BaseMover[] movers;

    public MoverToFirstFound(BaseUnit unit, BaseLevel level, params BaseMover[] movers) : base(unit, level)
    {
        this.movers = movers;
    }

    public override bool MoveUnit()
    {
        foreach (var mover in movers)
        {
            var res = mover.MoveUnit();
            if (res)
            {
                return true;
            }
        }

        return false;
    }

    public void Tick()
    {
        this.MoveUnit();
    }
}
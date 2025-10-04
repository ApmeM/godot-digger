using System.Threading.Tasks;
using BrainAI.AI;

public class MoverToFirstFound : BaseMover, IAITurn
{
    private readonly BaseMover[] movers;

    public MoverToFirstFound(BaseUnit unit, BaseLevel level, params BaseMover[] movers) : base(unit, level)
    {
        this.movers = movers;
    }

    public override async Task<bool> MoveUnit()
    {
        foreach (var mover in movers)
        {
            var res = await mover.MoveUnit();
            if (res)
            {
                return true;
            }
        }

        return false;
    }


    private bool taskInProgress = false;
    public async void Tick()
    {
        if (taskInProgress)
        {
            return;
        }

        taskInProgress = true;
        await this.MoveUnit();
        taskInProgress = false;
    }
}
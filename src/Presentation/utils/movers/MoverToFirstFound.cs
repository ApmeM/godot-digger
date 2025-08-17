using Godot;

public class MoverToFirstFound : BaseMover
{
    private readonly BaseMover[] movers;

    public MoverToFirstFound(params BaseMover[] movers)
    {
        this.movers = movers;
    }

    public override Vector2? MoveUnit(BaseUnit unit)
    {
        foreach (var mover in movers)
        {
            var res = mover.MoveUnit(unit);
            if (res != null)
            {
                return res;
            }
        }

        return null;
    }
}
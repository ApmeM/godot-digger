public abstract class BaseMover
{
    protected readonly BaseUnit unit;
    protected readonly BaseLevel level;

    public BaseMover(BaseUnit unit, BaseLevel level)
    {
        this.unit = unit;
        this.level = level;
    }

    public bool TryMoveUnit()
    {
        if (!Godot.Object.IsInstanceValid(unit) || !Godot.Object.IsInstanceValid(level))
        {
            return false;
        }

        return MoveUnit();
    }

    public abstract bool MoveUnit();
}

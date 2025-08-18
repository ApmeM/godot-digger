using Godot;

public class MoverToConstant : BaseMover
{
    private readonly Vector2[] follow;
    private int currentIndex;
    public MoverToConstant(BaseUnit unit, BaseLevel level, params Vector2[] follow) : base(unit, level)
    {
        this.follow = follow;
    }

    public override bool MoveUnit()
    {
        currentIndex++;
        if (this.currentIndex == this.follow.Length)
        {
            return false;
        }
        
        unit.StartMoveAction(this.follow[currentIndex]);
        return true;
    }
}
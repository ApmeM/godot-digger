using System.Threading.Tasks;
using Godot;

public class MoverToConstant : BaseMover
{
    private readonly Vector2[] follow;
    private int currentIndex;
    public MoverToConstant(BaseUnit unit, BaseLevel level, params Vector2[] follow) : base(unit, level)
    {
        this.follow = follow;
    }

    public override async Task<bool> MoveUnit()
    {
        currentIndex++;
        if (this.currentIndex >= this.follow.Length)
        {
            return false;
        }
        
        await unit.StartMoveAction(this.follow[currentIndex]);
        return true;
    }
}
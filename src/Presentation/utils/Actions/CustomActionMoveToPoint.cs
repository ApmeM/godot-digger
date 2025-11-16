using System;
using BrainAI.AI.UtilityAI;
using Godot;
using MonoCustomResourceRegistry;


[Serializable]
[Tool]
[RegisteredType(nameof(CustomActionMoveToPoint))]
public class CustomActionMoveToPoint : Resource, IIntent<BaseUnit>
{
    [Export]
    public Vector2 TargetPoint;

    public void Enter(BaseUnit context)
    {
    }

    public bool Execute(BaseUnit context)
    {
        context.StartMoveAnimation();
        context.UpdateAnimationDirection(this.TargetPoint - context.Position);
        var speed = context.MoveSpeed * context.Delta;
        var dir = (this.TargetPoint - context.Position).Normalized();
        var sqDistanceLeft = (context.Position - this.TargetPoint).LengthSquared();
        if (sqDistanceLeft < speed * speed)
        {
            context.Position = this.TargetPoint;
            return true;
        }

        context.Position += dir * speed;
        return false;
    }

    public void Exit(BaseUnit context)
    {
        context.StartStayAnimation();
    }
}

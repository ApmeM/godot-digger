using System;
using BrainAI.AI.UtilityAI;
using Godot;
using MonoCustomResourceRegistry;

[Serializable]
[Tool]
[RegisteredType(nameof(CustomActionFollowPath))]
public class CustomActionFollowPath : Resource, IIntent<BaseUnit>
{
    [Export]
    public float MoveOffset;

    [Export]
    public NodePath MagePath;

    [Export]
    public NodePath FollowPath { get; internal set; }

    public void Enter(BaseUnit context)
    {
    }

    public bool Execute(BaseUnit context)
    {
        var mage = context.GetNode<BaseUnit>(this.MagePath);
        var pathPosition = context.GetNode<PathFollow2D>(this.FollowPath);

        context.StartMoveAnimation();
        this.MoveOffset += context.MoveSpeed * context.Delta * mage.Character.EnemySpeedCoeff;
        pathPosition.Offset = this.MoveOffset;
        var oldPosition = context.Position;
        context.Position = pathPosition.Position;
        context.UpdateAnimationDirection(context.Position - oldPosition);
        return pathPosition.UnitOffset == 1;
    }

    public void Exit(BaseUnit context)
    {
        context.StartStayAnimation();
    }
}

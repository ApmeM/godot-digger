using System;
using BrainAI.AI.UtilityAI;
using Godot;
using MonoCustomResourceRegistry;

[Serializable]
[Tool]
[RegisteredType(nameof(CustomActionAttackOpponent))]
public class CustomActionAttackOpponent : Resource, IIntent<BaseUnit>
{
    [Export]
    public NodePath OpponentPath;

    [Export]
    public float AttackStep;

    public void Enter(BaseUnit context)
    {
    }

    public bool Execute(BaseUnit context)
    {
        var opponent = context.GetNodeOrNull<BaseUnit>(this.OpponentPath);
        if (!Godot.Object.IsInstanceValid(opponent))
        {
            return true;
        }

        context.StartAttackAnimation();
        context.UpdateAnimationDirection(opponent.Position - context.Position);
        var isHit = false;

        var animationTimePassed = context.GetAnimationTimePassed();
        if (this.AttackStep == 0 && context.HitDelay == 0)
        {
            isHit = true;
        }

        if (this.AttackStep < context.HitDelay && this.AttackStep + context.Delta >= context.HitDelay)
        {
            isHit = true;
        }

        this.AttackStep += context.Delta;

        if (isHit)
        {
            if (context.Projectile != null)
            {
                var instance = context.Projectile.Instance<BaseProjectile>();
                context.GetParent().AddChild(instance);
                instance.Shoot(context, opponent);
                instance.ZIndex = context.ZIndex;
            }
            else
            {
                opponent.GotHit(context);
            }
        }

        return this.AttackStep >= context.AttackDelay;
    }

    public void Exit(BaseUnit context)
    {
        context.StartStayAnimation();
    }
}

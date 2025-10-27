using System;
using System.Collections.Generic;
using BrainAI.AI;
using BrainAI.AI.UtilityAI;
using Godot;

[SceneReference("BaseUnit.tscn")]
public partial class BaseUnit : IIntentContainer<BaseUnit>
{
    #region Attack

    [Export]
    public int AttackPower;

    [Export]
    public float AttackDistance;

    [Export]
    public float AttackDelay
    {
        get
        {
            if (this.attackDelay < 0 && this.IsInsideTree())
            {
                this.attackDelay = RecalculateAttackDelay();
            }

            return attackDelay;
        }
        set
        {
            attackDelay = value;

            if (this.IsInsideTree())
            {
                if (attackDelay < 0)
                {
                    this.attackDelay = RecalculateAttackDelay();
                }
                var animations = this.animatedSprite.Frames.GetAnimationNames();
                foreach (var animation in animations)
                {
                    if (!animation.StartsWith("Attack"))
                    {
                        continue;
                    }

                    var framesCount = this.animatedSprite.Frames.GetFrameCount(animation);
                    this.animatedSprite.Frames.SetAnimationSpeed(animation, framesCount / attackDelay);

                    var newAnim = (Animation)this.animationPlayer.GetAnimation(animation).Duplicate();

                    newAnim.Length = this.attackDelay;
                    this.animationPlayer.AddAnimation(animation, newAnim);
                }
            }
        }
    }

    private float RecalculateAttackDelay()
    {
        var animations = this.animatedSprite.Frames.GetAnimationNames();
        var maxAnimationLength = float.MaxValue;

        foreach (var animation in animations)
        {
            if (!animation.StartsWith("Attack"))
            {
                continue;
            }

            var framesCount = this.animatedSprite.Frames.GetFrameCount(animation);
            var fps = this.animatedSprite.Frames.GetAnimationSpeed(animation);
            var delay = framesCount / fps;
            if (maxAnimationLength == float.MaxValue)
            {
                maxAnimationLength = delay;
            }
            else if (maxAnimationLength != delay)
            {
                GD.Print($"AttackAnimations have different lengths: {delay} and {maxAnimationLength}");
                maxAnimationLength = Math.Max(maxAnimationLength, delay);
            }
        }
        return maxAnimationLength;
    }

    private float attackDelay = -1;

    [Export]
    public float HitDelay;

    [Export]
    public PackedScene Projectile;

    #endregion

    #region Move

    [Export]
    public string PathFollow2DPath;

    [Export]
    public float MoveSpeed;

    #endregion

    #region Loot

    [Export]
    public List<PackedScene> Loot = new List<PackedScene>();

    [Export]
    public bool GrabLoot;

    #endregion

    #region Defence

    [Export]
    public bool ShowDeath;

    private uint maxHP;

    // HP order matters. Do not change it.
    [Export]
    public uint MaxHP
    {
        get
        {
            if (IsInsideTree())
            {
                return this.healthbar.MaxHP;
            }

            return maxHP;
        }
        set
        {
            this.maxHP = value;
            if (IsInsideTree())
            {
                this.healthbar.MaxHP = value;
            }
        }
    }

    private uint hp;

    // HP order matters. Do not change it.
    [Export]
    public uint HP
    {
        get
        {
            if (IsInsideTree())
            {
                return this.healthbar.CurrentHP;
            }

            return hp;
        }
        set
        {
            this.hp = value;
            if (IsInsideTree())
            {
                this.healthbar.CurrentHP = value;
            }
        }
    }

    #endregion

    public IAITurn AutomaticActionGenerator;
    public IContext AutomaticActionGeneratorContext;

    public IIntent<BaseUnit> Intent { get; set; }

    [Signal]
    public delegate void Clicked();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Unit);
        // HP order matters. Do not change it.
        this.MaxHP = this.maxHP;
        // HP order matters. Do not change it.
        this.HP = this.hp;
        this.AttackDelay = this.attackDelay;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        AutomaticActionGeneratorContext?.Update(delta);
        AutomaticActionGenerator?.Tick();
    }

    private void StartAnimation(string animation)
    {
        var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        stateMachine.Travel(animation);
    }

    public void StartGrabLootAnimation()
    {
        StartAnimation("Grab");
        // BaseLoot l
        // this.Loot.Add(Instantiator.LoadLoot(l.LootName));
        // l.QueueFree();
    }

    public void StartMoveAnimation()
    {
        StartAnimation("Move");
    }

    public void StartStayAnimation()
    {
        StartAnimation("Stay");
    }

    public void StartAttackAnimation()
    {
        StartAnimation("Attack");
    }

    public void UpdateAnimationDirection(Vector2 dir)
    {
        this.animationTree.Set("parameters/Attack/blend_position", new Vector2(dir.x, -dir.y));
        this.animationTree.Set("parameters/Move/blend_position", new Vector2(dir.x, -dir.y));
        this.animationTree.Set("parameters/Stay/blend_position", new Vector2(dir.x, -dir.y));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouse && mouse.IsPressed() && !mouse.IsEcho() && (ButtonList)mouse.ButtonIndex == ButtonList.Left)
        {
            var frame = animatedSprite.Frames.GetFrame(animatedSprite.Animation, animatedSprite.Frame);
            if (frame != null)
            {
                var size = frame.GetSize() * animatedSprite.Scale;
                var position = this.animatedSprite.Position + this.animatedSprite.Offset - (this.animatedSprite.Centered ? size / 2 : Vector2.Zero);

                var rect = new Rect2(position, size);
                var mousePos = this.GetLocalMousePosition();

                if (rect.HasPoint(mousePos))
                {
                    this.GetTree().SetInputAsHandled();
                    this.EmitSignal(nameof(Clicked));
                }
            }
        }
    }
}

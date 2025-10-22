using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public List<string> AggroAgainst;

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

    [Signal]
    public delegate void LootDropped(BaseLoot loot);

    [Export]
    public List<PackedScene> Loot = new List<PackedScene>();

    [Export]
    public bool GrabLoot;

    #endregion

    #region CustomActions

    [Export]
    public PackedScene SpawnUnit;

    #endregion

    #region Defence

    [Signal]
    public delegate void OnHit();

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

    #region Level

    [Export]
    public NodePath LevelPath;

    private BaseLevel internalLevel;
    public BaseLevel level
    {
        get
        {
            internalLevel = internalLevel ?? this.GetNode<BaseLevel>(LevelPath);
            return internalLevel;
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

    public void StartGrabLootAnimation()
    {
        StartAnimation("Grab");
        // BaseLoot l
        // this.Loot.Add(Instantiator.LoadLoot(l.LootName));
        // l.QueueFree();
    }

    private void StartAnimation(string animation)
    {
        var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        stateMachine.Travel(animation);
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

    public void UpdateAniationDirection(Vector2 dir)
    {
        this.animationTree.Set("parameters/Attack/blend_position", new Vector2(dir.x, -dir.y));
        this.animationTree.Set("parameters/Move/blend_position", new Vector2(dir.x, -dir.y));
        this.animationTree.Set("parameters/Stay/blend_position", new Vector2(dir.x, -dir.y));
    }

    public async Task StartMoveAction(Vector2 destination)
    {
        StartMoveAnimation();
        UpdateAniationDirection((destination - this.Position).Normalized());

        float distance = this.Position.DistanceTo(destination);
        float duration = distance / MoveSpeed / level.HeaderControl.Character.EnemySlowdownCoeff;

        var tween = this.CreateTween();
        tween.TweenProperty(this, "position", destination, duration)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        await tween.ToMySignal(CommonSignals.Finished);
    }

    public async Task StartAttackAction(BaseUnit opponent, Action onHit = null)
    {
        StartAttackAnimation();
        UpdateAniationDirection((opponent.Position - this.Position).Normalized());

        await this.GetTree().CreateTimer(this.HitDelay).ToMySignal(CommonSignals.Timeout);

        if (!Godot.Object.IsInstanceValid(this) || !Godot.Object.IsInstanceValid(opponent))
        {
            // Either attacker or defender no longer on a scene. 
            // No need to calculate attcks.
            return;
        }

        onHit = onHit ?? (() => opponent.GotHit(this, this.AttackPower));

        if (this.Projectile != null)
        {
            var instance = this.Projectile.Instance<BaseProjectile>();
            this.GetParent().AddChild(instance);
            instance.Shoot(this.Position, opponent, onHit);
            instance.ZIndex = this.ZIndex;
        }
        else
        {
            onHit.Invoke();
        }

        await this.GetTree().CreateTimer(Math.Max(0, this.AttackDelay - this.HitDelay)).ToMySignal(CommonSignals.Timeout);

        if (!Godot.Object.IsInstanceValid(this))
        {
            // Either attacker or defender no longer on a scene. 
            // No need to calculate attcks.
            return;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouse && mouse.IsPressed() && !mouse.IsEcho() && (ButtonList)mouse.ButtonIndex == ButtonList.Left)
        {
            var size = animatedSprite.Frames.GetFrame(animatedSprite.Animation, animatedSprite.Frame).GetSize() * animatedSprite.Scale;
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

    public void GotHit(BaseUnit from, int attackPower)
    {
        if (!Godot.Object.IsInstanceValid(this))
        {
            return;
        }

        var hitPower = Math.Min(attackPower, this.HP);
        this.HP = (uint)Math.Max(0, this.HP - hitPower);
        level.FloatingTextManagerControl.ShowValue((-hitPower).ToString(), this.Position, new Color(1, 0, 0));

        if (from != null && Godot.Object.IsInstanceValid(from))
        {
            var enemyGroups = from.GetGroups()
                .Cast<string>()
                .Where(a => a.StartsWith(Groups.AggrouGroupPrefix))
                .ToArray();

            var myGroups = this.GetGroups()
                .Cast<string>()
                .Where(a => a.StartsWith(Groups.AggrouGroupPrefix))
                .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
                .ToHashSet();

            foreach (var unit in myGroups)
            {
                unit.AggroAgainst = (unit.AggroAgainst ?? new List<string>()).Union(enemyGroups).ToList();
                foreach (var enemy in enemyGroups)
                {
                    unit.AggroAgainst.Add(enemy);
                }
            }
        }

        if (this.SpawnUnit != null)
        {
            if (LevelPath == null)
            {
                GD.PrintErr($"Should spawn unit on attack, but LevelPath is not set.");
            }
            else
            {
                var instance = this.SpawnUnit.Instance<BaseUnit>();
                instance.Position = this.Position;
                instance.LevelPath = this.LevelPath;
                instance.AggroAgainst = this.AggroAgainst;
                foreach (var group in this.GetGroups().Cast<string>().Where(a => a.StartsWith(Groups.AggrouGroupPrefix)))
                {
                    instance.AddToGroup(group);
                }
                this.GetParent().AddChild(instance);
            }
        }

        this.EmitSignal(nameof(OnHit));

        if (this.HP <= 0)
        {
            if (ShowDeath)
            {
                level.FloatingTextManagerControl.ShowValue(Instantiator.CreateBuff(nameof(Dead)), this.Position);
            }
            this.DropLoot();
            this.QueueFree();
        }
    }

    public void DropLoot()
    {
        if (LevelPath == null)
        {
            GD.PrintErr($"Should drop loot, but LevelPath is not set.");
            return;
        }
        var loots = Loot;

        foreach (var loot in loots)
        {
            var newLoot = loot.Instance<BaseLoot>();
            newLoot.LevelPath = this.LevelPath;
            newLoot.Position = this.Position;
            this.GetParent().AddChild(newLoot);
            this.EmitSignal(nameof(LootDropped), newLoot);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[SceneReference("BaseUnit.tscn")]
public partial class BaseUnit
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
    public float MoveSpeed;

    [Export]
    public float MoveDelay;

    [Export]
    public List<Floor> MoveFloors { get => moveFloors; set  { moveFloors = value; moveFloorsSet = value.ToHashSet(); } }
    private List<Floor> moveFloors;
    private HashSet<Floor> moveFloorsSet = new HashSet<Floor>();
    public HashSet<Floor> MoveFloorsSet{ get => moveFloorsSet; }

    [Export]
    public int VisionDistance = 10;

    public BaseMover AutomaticActionGenerator;

    #endregion

    #region Loot

    [Signal]
    public delegate void LootDropped(BaseLoot loot);

    [Export]
    public List<PackedScene> Loot = new List<PackedScene>();

    [Export]
    public bool GrabLoot;

    #endregion

    #region Chat

    [Export]
    public List<QuestData> QuestRequirements = new List<QuestData>();

    [Export]
    public List<QuestData> QuestRewards = new List<QuestData>();

    private string questContent;

    [Export]
    public string QuestContent
    {
        get
        {
            if (IsInsideTree())
            {
                return this.questPopup.Content;
            }

            return questContent;
        }
        set
        {
            this.questContent = value;
            if (IsInsideTree())
            {
                this.questPopup.Content = value;
            }
        }
    }

    private string signContent;

    [Export]
    public string SignContent
    {
        get
        {
            if (IsInsideTree())
            {
                return this.signLabel.Text;
            }

            return signContent;
        }
        set
        {
            this.signContent = value;
            if (IsInsideTree())
            {
                this.signLabel.Text = value;
            }
        }
    }

    #endregion

    #region CustomActions

    [Export]
    public string MoveToLevel { get; set; }

    [Export]
    public PackedScene SpawnUnit;

    #endregion

    #region Defence

    [Signal]
    public delegate void OnHit(int hpLeft);

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
    protected BaseLevel level
    {
        get
        {
            internalLevel = internalLevel ?? this.GetNode<BaseLevel>(LevelPath);
            return internalLevel;
        }
    }

    #endregion

    [Signal]
    public delegate void Clicked();

    private float currentActionDelay;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Unit);
        // HP order matters. Do not change it.
        this.MaxHP = this.maxHP;
        // HP order matters. Do not change it.
        this.HP = this.hp;
        this.QuestContent = this.questContent;
        this.SignContent = this.signContent;
        this.AttackDelay = this.attackDelay;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (this.currentActionDelay >= 0)
        {
            this.currentActionDelay -= delta;
            return;
        }

        if (AutomaticActionGenerator?.MoveUnit() == true)
        {
            return;
        }

        StartStayAction();
    }

    public void CancelAction()
    {
        this.currentActionDelay = 0;
    }

    public async void StartGrabLoot(BaseLoot l)
    {
        if (currentActionDelay > 0)
        {
            // Previous action not done. 
            // Should not start new one.
            return;
        }

        // TODO: Add animation to grab loot
        // var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        // stateMachine.Travel("Grab");

        this.Loot.Add(Instantiator.LoadLoot(l.LootName));
        l.QueueFree();
    }

    public async void StartStayAction()
    {
        if (currentActionDelay > 0)
        {
            // Previous action not done. 
            // Should not start new one.
            return;
        }

        var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        stateMachine.Travel("Stay");
    }

    public async void StartMoveAction(Vector2 destination)
    {
        if (currentActionDelay > 0)
        {
            // Previous action not done. 
            // Should not start new one.
            return;
        }

        var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        var dir = (destination - this.Position).Normalized();
        this.animationTree.Set("parameters/Move/blend_position", new Vector2(dir.x, -dir.y));
        this.animationTree.Set("parameters/Stay/blend_position", new Vector2(dir.x, -dir.y));
        stateMachine.Travel("Move");

        float distance = this.Position.DistanceTo(destination);
        float duration = distance / MoveSpeed;

        var tween = this.CreateTween();
        tween.TweenProperty(this, "position", destination, duration)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        this.currentActionDelay = duration;
        try
        {
            await this.ToSignal(tween, CommonSignals.Finished);
        }
        catch (ObjectDisposedException)
        {
            // Either attacker or defender no longer on a scene. 
            // No need to calculate attcks.
            return;
        }
        if (!Godot.Object.IsInstanceValid(this))
        {
            // Either attacker or defender no longer on a scene. 
            // No need to calculate attcks.
            return;
        }
    }

    public async void StartAttackAction(BaseUnit opponent, Action onHit = null)
    {
        if (currentActionDelay > 0)
        {
            // Previous action not done. 
            // Should not start new one.
            return;
        }
        var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        var dir = (opponent.Position - this.Position).Normalized();
        this.animationTree.Set("parameters/Attack/blend_position", new Vector2(dir.x, -dir.y));
        this.animationTree.Set("parameters/Stay/blend_position", new Vector2(dir.x, -dir.y));
        stateMachine.Travel("Attack");

        this.currentActionDelay = this.AttackDelay + 0.1f;
        await this.ToSignal(this.GetTree().CreateTimer(this.HitDelay), CommonSignals.Timeout);
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
        }
        else
        {
            onHit.Invoke();
        }

        await this.ToSignal(this.GetTree().CreateTimer(Math.Max(0, this.AttackDelay - this.HitDelay)), CommonSignals.Timeout);
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
                this.UnitClicked();
                this.EmitSignal(nameof(Clicked));
            }
        }
    }

    public virtual void UnitClicked()
    {
        if (level.HeaderControl.Character.CanDig && this.MaxHP > 0)
        {
            DigClick();
        }

        if (!string.IsNullOrWhiteSpace(this.MoveToLevel))
        {
            MoveToLevelClick();
        }

        if (!string.IsNullOrWhiteSpace(this.QuestContent))
        {
            QuestClick();
        }
        else if (!string.IsNullOrWhiteSpace(this.SignContent))
        {
            SignClick();
        }
    }

    private async void QuestClick()
    {
        this.questPopup.BagInventoryPath = level.BagInventoryPopup.GetPath();

        var result = await questPopup.ShowQuestPopup(
            QuestContent,
            QuestRequirements,
            QuestRewards
        );

        if (result && !string.IsNullOrWhiteSpace(this.SignContent))
        {
            SignClick();
        }
    }

    private void SignClick()
    {
        this.signPopup.Show();
    }

    private void MoveToLevelClick()
    {
        level.EmitSignal(nameof(BaseLevel.ChangeLevel), MoveToLevel);
    }

    public void DigClick()
    {
        var worldPos = this.Position;

        if (level.HeaderControl.CurrentStamina == 0)
        {
            level.FloatingTextManagerControl.ShowValue("Too tired", worldPos, new Color(0.60f, 0.85f, 0.91f));
            return;
        }

        level.HeaderControl.CurrentStamina--;
        level.FloatingTextManagerControl.ShowValue((-1).ToString(), worldPos, new Color(0.60f, 0.85f, 0.91f));

        this.GotHit(null, (int)level.HeaderControl.Character.DigPower);

        if (this.AttackPower > 0 && this.HP > 0)
        {
            DefendClick();
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

        if (from != null)
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

        this.EmitSignal(nameof(OnHit), this.HP);

        if (this.HP <= 0)
        {
            if (ShowDeath)
            {
                level.FloatingTextManagerControl.ShowValue(Instantiator.CreateBuff(Buff.Dead), this.Position);
            }
            this.DropLoot();
            this.QueueFree();
        }
    }

    private void DefendClick()
    {
        var hitPower = (uint)Math.Min(this.AttackPower, level.HeaderControl.CurrentHp);
        level.HeaderControl.CurrentHp -= hitPower;
        level.FloatingTextManagerControl.ShowValue((-hitPower).ToString(), this.Position, new Color(1, 0, 0));

        if (level.HeaderControl.CurrentHp <= 0)
        {
            level.HeaderControl.AddBuff(Buff.Dead);
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

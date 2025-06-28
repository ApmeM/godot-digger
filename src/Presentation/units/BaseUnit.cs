using System;
using System.Collections.Generic;
using System.Linq;
using BrainAI.Pathfinding;
using Godot;

[SceneReference("BaseUnit.tscn")]
[Tool]
public partial class BaseUnit
{
    #region Attack

    [Export]
    public int AttackPower;

    [Export]
    public float AttackDistance;

    [Export]
    public float AttackDelay;

    [Export]
    public float HitDelay;

    [Export]
    public List<string> AggroAgainst;

    #endregion

    #region Move

    [Export]
    public float MoveSpeed;

    [Export]
    public float MoveDelay;

    [Export]
    public List<Floor> MoveFloors;

    [Export]
    public int VisionDistance = 10;

    private List<(Vector2, HashSet<Floor>)> moveResultPath = new List<(Vector2, HashSet<Floor>)>();

    private Vector2? moveNextStep;

    private IPathfinder<(Vector2, HashSet<Floor>)> internalMovePathfinder;
    private IPathfinder<(Vector2, HashSet<Floor>)> movePathfinder
    {
        get
        {
            internalMovePathfinder = internalMovePathfinder ?? new WeightedPathfinder<(Vector2, HashSet<Floor>)>(level);
            return internalMovePathfinder;
        }
    }

    #endregion

    #region Loot

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

    private static Random random = new Random();

    private float currentActionDelay;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        var animations = this.animatedSprite.Frames.GetAnimationNames();
        foreach (var animation in animations)
        {
            var framesCount = this.animatedSprite.Frames.GetFrameCount(animation);
            var fps = this.animatedSprite.Frames.GetAnimationSpeed(animation);
            var newAnim = (Animation)this.animationPlayer.GetAnimation(animation).Duplicate();
            newAnim.Length = framesCount / fps;
            this.animationPlayer.AddAnimation(animation, newAnim);
        }
        var newAttackDelay = this.animationPlayer.GetAnimation("AttackTop").Length;
        if (newAttackDelay > this.AttackDelay)
        {
            GD.PrintErr($"{this.GetType()}: Calculated attack delay ({newAttackDelay}) is bigger then specified {this.AttackDelay}. This may lead to skipped animations. Reset to calcualted value.");
            this.AttackDelay = newAttackDelay;
        }

        this.AddToGroup(Groups.Unit);
        // HP order matters. Do not change it.
        this.MaxHP = this.maxHP;
        // HP order matters. Do not change it.
        this.HP = this.hp;
        this.QuestContent = this.questContent;
        this.SignContent = this.signContent;
    }

    private BaseUnit opponent = null;

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (this.LevelPath == null)
        {
            return;
        }

        var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        currentActionDelay -= delta;
        if (currentActionDelay >= 0)
        {
            if (currentActionDelay <= this.HitDelay && this.HitDelay < currentActionDelay + delta && opponent != null)
            {
                opponent.GotHit(this, this.AttackPower);
                opponent = null;
            }
            return;
        }

        if (this.AttackPower > 0 && this.AggroAgainst != null && this.AggroAgainst.Count > 0)
        {
            opponent = this.AggroAgainst
                .Except(this.GetGroups().Cast<string>())
                .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
                .Where(a => (a.Position - this.Position).LengthSquared() < this.AttackDistance * this.AttackDistance)
                .OrderBy(a => (a.Position - this.Position).LengthSquared())
                .FirstOrDefault();
            if (opponent != null)
            {
                var dir = (opponent.Position - this.Position).Normalized();
                this.animationTree.Set("parameters/Attack/blend_position", new Vector2(dir.x, -dir.y));
                this.animationTree.Set("parameters/Stay/blend_position", new Vector2(dir.x, -dir.y));
                stateMachine.Travel("Attack");

                currentActionDelay = AttackDelay + 0.1f;
                return;
            }
        }

        if (this.MoveSpeed > 0)
        {
            if (this.MoveFloors == null || MoveFloors.Count == 0)
            {
                GD.PrintErr($"Should move but have no floors defined : {this.GetType()} {this.GetPath()}");
                return;
            }
            if (moveNextStep != null)
            {
                var dir = (moveNextStep.Value - this.Position).Normalized();
                this.animationTree.Set("parameters/Move/blend_position", new Vector2(dir.x, -dir.y));
                this.animationTree.Set("parameters/Stay/blend_position", new Vector2(dir.x, -dir.y));
                stateMachine.Travel("Move");

                var speed = this.MoveSpeed * delta;
                var direction = moveNextStep.Value - this.Position;
                if (direction.LengthSquared() > speed * speed)
                {
                    this.Position += direction.Normalized() * speed;
                    return;
                }

                this.Position = moveNextStep.Value;
                moveNextStep = null;
                currentActionDelay = this.MoveDelay;

                if (this.GrabLoot)
                {
                    var loots = this.GetTree()
                        .GetNodesInGroup(Groups.Loot)
                        .Cast<BaseLoot>()
                        .Where(a => level.WorldToMap(a.Position) == level.WorldToMap(this.Position))
                        .ToList();

                    foreach (var l in loots)
                    {
                        this.Loot.Add(Instantiator.LoadLoot(l.LootName));
                        l.QueueFree();
                    }
                }

                return;
            }

            var floorsSet = MoveFloors.ToHashSet();

            this.moveNextStep = this.GetPathToLoot(floorsSet, VisionDistance) ??
                        this.GetPathToOtherGroup(floorsSet, VisionDistance) ??
                        this.GetPathToRandomLocation(floorsSet);
            if (this.moveNextStep == null)
            {
                moveNextStep = null;
                currentActionDelay = MoveDelay;
                return;
            }
            moveNextStep = level.MapToWorld(moveNextStep.Value);
        }
    }

    protected Vector2? GetPathToRandomLocation(HashSet<Floor> floors)
    {
        var pos = level.WorldToMap(this.Position);
        var dest = pos + level.moveDirections[random.Next(level.moveDirections.Length)];
        if (!level.IsReachable(dest, floors))
        {
            return null;
        }

        moveResultPath.Clear();
        movePathfinder.Search((pos, floors), (dest, floors), 10, moveResultPath);

        if (moveResultPath == null || moveResultPath.Count < 2)
        {
            return null;
        }

        return moveResultPath[1].Item1;
    }

    protected Vector2? GetPathToLoot(HashSet<Floor> floors, int maxDistance)
    {
        if (!this.GrabLoot)
        {
            return null;
        }

        var pos = level.WorldToMap(this.Position);

        var loots = this.GetTree()
            .GetNodesInGroup(Groups.Loot)
            .Cast<BaseLoot>()
            .Select(a => (level.WorldToMap(a.Position), floors))
            .Where(a => level.IsReachable(a.Item1, floors))
            .Where(a => (a.Item1 - pos).LengthSquared() <= maxDistance * maxDistance)
            .ToHashSet();

        moveResultPath.Clear();
        movePathfinder.Search((pos, floors), loots, (maxDistance * 2 + 1) * (maxDistance * 2 + 1), moveResultPath);

        if (moveResultPath == null || moveResultPath.Count < 2)
        {
            return null;
        }

        return moveResultPath[1].Item1;
    }

    protected Vector2? GetPathToOtherGroup(HashSet<Floor> floors, int maxDistance)
    {
        if (this.AggroAgainst == null || this.AggroAgainst.Count == 0)
        {
            return null;
        }

        var pos = level.WorldToMap(this.Position);
        var otherGroups = this.AggroAgainst
            .Except(this.GetGroups().Cast<string>())
            .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Select(a => (level.WorldToMap(a.Position), floors))
            .Where(a => level.IsReachable(a.Item1, floors))
            .Where(a => (a.Item1 - pos).LengthSquared() <= maxDistance * maxDistance)
            .ToHashSet();

        moveResultPath.Clear();
        movePathfinder.Search((pos, floors), otherGroups, (maxDistance * 2 + 1) * (maxDistance * 2 + 1), moveResultPath);

        if (moveResultPath == null || moveResultPath.Count < 2)
        {
            return null;
        }

        return moveResultPath[1].Item1;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouse && mouse.IsPressed() && !mouse.IsEcho() && (ButtonList)mouse.ButtonIndex == ButtonList.Left)
        {
            var size = animatedSprite.Frames.GetFrame(animatedSprite.Animation, animatedSprite.Frame).GetSize();
            var rect = new Rect2(this.animatedSprite.Position, size);
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
        var hitPower = (uint)Math.Min(attackPower, this.HP);
        this.HP -= hitPower;
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
        var loots = Loot;

        foreach (var loot in loots)
        {
            var newLoot = loot.Instance<BaseLoot>();
            newLoot.LevelPath = this.LevelPath;
            newLoot.Position = this.Position;
            this.GetParent().AddChild(newLoot);
        }
    }
}

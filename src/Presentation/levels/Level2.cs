using System;
using System.Collections.Generic;
using System.Linq;
using BrainAI.AI.UtilityAI;
using Godot;

public interface IContext
{
    float Delta { get; set; }
    void Update(float delta);
}
public class EnemyContext : IContext
{
    public float Delta { get; set; }

    public void Update(float delta)
    {
        this.Delta = delta;
    }
}
public class TowerContext : IContext
{
    public float Delta { get; set; }

    public void Update(float delta)
    {
        this.Delta = delta;
    }
}
public class CanAttackUnitAppraisal : IAppraisal<BaseUnit>
{
    private readonly BaseUnit unit;

    public CanAttackUnitAppraisal(BaseUnit unit)
    {
        this.unit = unit;
    }

    public float GetScore(BaseUnit context)
    {
        return ((this.unit.Position - context.Position).LengthSquared() < context.AttackDistance * context.AttackDistance) ? 1 : 0;
    }
}
public class UnitInGroupAppraisal : IAppraisal<BaseUnit>
{
    private string group;

    public UnitInGroupAppraisal(string group)
    {
        this.group = group;
    }

    public float GetScore(BaseUnit context)
    {
        return context.IsInGroup(this.group) ? 1 : 0;
    }
}
public class FollowPathIntent : IIntent<BaseUnit>
{
    public float MoveOffset;

    public void Enter(BaseUnit context)
    {
        context.StartMoveAnimation();
        this.MoveOffset = 0;
    }

    public bool Execute(BaseUnit context)
    {
        var moveContext = (EnemyContext)context.AutomaticActionGeneratorContext;
        this.MoveOffset += context.MoveSpeed * moveContext.Delta;
        var pathPosition = (PathFollow2D)context.GetNode(context.PathFollow2DPath);
        pathPosition.Offset = this.MoveOffset;
        var oldPosition = context.Position;
        context.Position = pathPosition.Position;
        context.UpdateAniationDirection(context.Position - oldPosition);
        return pathPosition.UnitOffset == 1;
    }

    public void Exit(BaseUnit context)
    {
        context.StartStayAnimation();
    }
}
public class MoveToPointIntent : IIntent<BaseUnit>
{
    private readonly Vector2 TargetPoint;

    public MoveToPointIntent(Vector2 targetPoint)
    {
        this.TargetPoint = targetPoint;
    }
    public void Enter(BaseUnit context)
    {
        context.StartMoveAnimation();
        context.UpdateAniationDirection(this.TargetPoint - context.Position);
    }

    public bool Execute(BaseUnit context)
    {
        var moveContext = (TowerContext)context.AutomaticActionGeneratorContext;
        var speed = context.MoveSpeed * moveContext.Delta;
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
public class AttackOpponentIntent : IIntent<BaseUnit>
{
    private readonly BaseUnit opponent;
    private Action onHit;
    private float AttackStep;

    public AttackOpponentIntent(BaseUnit opponent, Action onHit = null)
    {
        this.opponent = opponent;
        this.onHit = onHit;
    }

    public void Enter(BaseUnit context)
    {
        context.StartAttackAnimation();
        this.AttackStep = 0;
    }
    public bool Execute(BaseUnit context)
    {
        var commonContext = context.AutomaticActionGeneratorContext;
        var isHit = false;
        if (this.AttackStep == 0 && context.HitDelay == 0)
        {
            isHit = true;
        }

        if (this.AttackStep < context.HitDelay && this.AttackStep + commonContext.Delta >= context.HitDelay)
        {
            isHit = true;
        }

        this.AttackStep += commonContext.Delta;

        if (isHit)
        {
            this.onHit = this.onHit ?? (() => opponent.GotHit(context, context.AttackPower));

            if (context.Projectile != null)
            {
                var instance = context.Projectile.Instance<BaseProjectile>();
                context.GetParent().AddChild(instance);
                instance.Shoot(context.Position, opponent, onHit);
                instance.ZIndex = context.ZIndex;
            }
            else
            {
                onHit();
            }
        }

        return this.AttackStep >= context.AttackDelay;
    }

    public void Exit(BaseUnit context)
    {
        context.StartStayAnimation();
    }
}

[SceneReference("Level2.tscn")]
public partial class Level2
{
    private Random r = new Random();
    private int level = 0;

    private Vector2 leftTowerInitialPosition;
    private Vector2 rightTowerInitialPosition;
    private Vector2 centerTowerInitialPosition;

    private bool waveInProgress = false;
    private readonly Queue<BaseUnit> enemiesToSpawn = new Queue<BaseUnit>();
    private float spawnTimeout;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.mage.Connect(nameof(BaseUnit.OnHit), this, nameof(MageHit));

        this.leftTower.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { this.leftTower });
        this.rightTower.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { this.rightTower });
        this.centerTower.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { this.centerTower });

        // Actually towers do not have AI. They just do intents when they are set outside.
        var reasoner = new FirstScoreReasoner<BaseUnit>(1);
        reasoner.Add(new HasIntentAppraisal<BaseUnit>(1), new UseIntentAction<BaseUnit>());
        this.leftTower.AutomaticActionGeneratorContext = new TowerContext();
        this.leftTower.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.leftTower, reasoner);
        this.rightTower.AutomaticActionGeneratorContext = new TowerContext();
        this.rightTower.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.rightTower, reasoner);
        this.centerTower.AutomaticActionGeneratorContext = new TowerContext();
        this.centerTower.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.centerTower, reasoner);
        this.mage.AutomaticActionGeneratorContext = new TowerContext();
        this.mage.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.mage, reasoner);

        this.toBattle.Connect(CommonSignals.Pressed, this, nameof(StartWave));
        this.upgradeDoor.Connect(CommonSignals.Pressed, this, nameof(UpgradeMage));

        this.leftTowerInitialPosition = this.leftTower.Position;
        this.rightTowerInitialPosition = this.rightTower.Position;
        this.centerTowerInitialPosition = this.centerTower.Position;

        this.leftTower.AttackDelay = 0.3f;
        this.leftTower.HitDelay = 0.1f;
        this.rightTower.AttackDelay = 0.3f;
        this.rightTower.HitDelay = 0.1f;
        this.centerTower.AttackDelay = 0.3f;
        this.centerTower.HitDelay = 0.1f;
    }

    private void UpgradeMage()
    {
        var count = this.BagInventoryPopup.GetItemCount(nameof(Gold));
        if (count > this.mage.MaxHP * this.mage.MaxHP)
        {
            this.BagInventoryPopup.TryChangeCount(nameof(Gold), -(int)(this.mage.MaxHP * this.mage.MaxHP));
            this.mage.MaxHP++;
            this.mage.HP = this.mage.MaxHP;
        }
    }

    private void StartWave()
    {
        waveInProgress = true;

        timerLabel.ShowMessage($"Level {level + 1}.", 5);

        switch (level)
        {
            case 0:
                this.leftTower.Intent = new MoveToPointIntent(new Vector2(240, 740));
                this.rightTower.Intent = new MoveToPointIntent(this.rightTowerInitialPosition);
                this.centerTower.Intent = new MoveToPointIntent(this.centerTowerInitialPosition);
                break;
            case 1:
                this.leftTower.Intent = new MoveToPointIntent(new Vector2(50, 740));
                this.rightTower.Intent = new MoveToPointIntent(new Vector2(480 - 50, 740));
                this.centerTower.Intent = new MoveToPointIntent(this.centerTowerInitialPosition);
                break;
            default:
                this.leftTower.Intent = new MoveToPointIntent(new Vector2(50, 740));
                this.rightTower.Intent = new MoveToPointIntent(new Vector2(480 - 50, 740));
                this.centerTower.Intent = new MoveToPointIntent(new Vector2(240, 740));
                break;
        }

        this.mage.Intent = new MoveToPointIntent(new Vector2(255, 686));

        var numberOfEnemies = 10 + level * 20;
        var enemyTypes = new List<string>();
        if (level >= 0)
        {
            enemyTypes.Add(nameof(Wolf));
        }
        if (level >= 1)
        {
            enemyTypes.Add(nameof(Wasp));
        }
        if (level >= 2)
        {
            enemyTypes.Add(nameof(Slime));
        }

        var speed = 150 + level * 15;
        var startPosition = enemyPath.Curve.GetPointPosition(0);

        var reasoner = new FirstScoreReasoner<BaseUnit>(1);
        // Intent phase
        reasoner.Add(new HasIntentAppraisal<BaseUnit>(1), new UseIntentAction<BaseUnit>());
        // Decision phase
        reasoner.Add(new MultAppraisal<BaseUnit>(new UnitInGroupAppraisal(Groups.AttackingEnemy), new CanAttackUnitAppraisal(this.mage)), new SetIntentAction<BaseUnit, AttackOpponentIntent>((c) => new AttackOpponentIntent(this.mage)));
        reasoner.Add(new NotAppraisal<BaseUnit>(new CanAttackUnitAppraisal(this.mage)), new SetIntentAction<BaseUnit, FollowPathIntent>((c) => new FollowPathIntent()));

        var enemies = Enumerable
            .Range(0, numberOfEnemies)
            .Select(a => BuildEnemy(enemyTypes, startPosition, reasoner, speed))
            .ToList();

        enemiesToSpawn.Clear();
        spawnTimeout = float.MaxValue;
        foreach (var enemy in enemies)
        {
            enemiesToSpawn.Enqueue(enemy);
        }
    }
    private void TickWave(float delta)
    {
        if (spawnTimeout > 0.3f && enemiesToSpawn.Count > 0)
        {
            spawnTimeout = 0;
            var enemy = enemiesToSpawn.Dequeue();
            this.floor.AddChild(enemy);
        }

        spawnTimeout += delta;

        if (this.mage.HP <= 1)
        {
            this.StopWave();
        }
        else if (this.GetTree().GetNodesInGroup(Groups.Enemy).Count == 0)
        {
            level++;
            this.StopWave();
        }
    }
    private void StopWave()
    {
        waveInProgress = false;
        if (this.mage.HP <= 1)
        {
            timerLabel.ShowMessage($"Game Over.", 5);
        }
        else
        {
            timerLabel.ShowMessage($"Level clear.", 5);
        }

        this.mage.Intent = new MoveToPointIntent(new Vector2(297, 1004));

        this.mage.HP = this.mage.MaxHP;

        var enemies = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .ToList();
        foreach (var enemy in enemies)
        {
            enemy.QueueFree();
        }

        this.HeaderControl.ClearBuffs();

        this.leftTower.Intent = new MoveToPointIntent(this.leftTowerInitialPosition);
        this.rightTower.Intent = new MoveToPointIntent(this.rightTowerInitialPosition);
        this.centerTower.Intent = new MoveToPointIntent(this.centerTowerInitialPosition);
    }

    private BaseUnit BuildEnemy(List<string> enemies, Vector2 position, Reasoner<BaseUnit> action, int speed)
    {
        var enemyName = enemies[r.Next(enemies.Count)];
        var enemy = Instantiator.CreateUnit(enemyName);
        enemy.Position = position;
        enemy.LevelPath = this.GetPath();
        enemy.PathFollow2DPath = this.enemyPathFollow.GetPath();
        enemy.AggroAgainst = new List<string> { "grp_player" };
        enemy.AttackPower = 1;
        enemy.AttackDistance = 100;
        enemy.Loot = new List<PackedScene> { Instantiator.LoadLoot(nameof(Gold)) };
        enemy.MaxHP = 3;
        enemy.HP = 1;
        enemy.MoveSpeed = speed;
        enemy.ZIndex = 1;
        enemy.AttackDelay = 0.3f;
        enemy.HitDelay = enemy.AttackDelay / 2;
        enemy.AddToGroup(Groups.Enemy);
        enemy.AddToGroup(Groups.AttackingEnemy);
        enemy.Connect(nameof(BaseUnit.LootDropped), this, nameof(LootDropped));
        enemy.AutomaticActionGeneratorContext = new EnemyContext();
        enemy.AutomaticActionGenerator = new UtilityAI<BaseUnit>(enemy, action);
        return enemy;
    }

    private async void LootDropped(BaseLoot newLoot)
    {
        var toPosition = new Vector2(0, 0);
        var tween = newLoot.CreateTween();
        tween.TweenProperty(newLoot, "position", toPosition, 0.5f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        await tween.ToMySignal(CommonSignals.Finished);
        newLoot.LootClicked();
    }

    private void TowerClicked(BaseUnit tower)
    {
        if (tower.Intent != null)
        {
            return;
        }

        Type against;
        if (tower == this.leftTower)
        {
            against = typeof(Wolf);
        }
        else if (tower == this.rightTower)
        {
            against = typeof(Wasp);
        }
        else if (tower == this.centerTower)
        {
            against = typeof(Slime);
        }
        else
        {
            throw new Exception("Unkonwn tower");
        }

        var enemy = this.GetTree()
            .GetNodesInGroup(Groups.AttackingEnemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.mage.Position).LengthSquared())
            .FirstOrDefault();

        if (enemy == null)
        {
            return;
        }

        if (against == enemy.GetType())
        {
            tower.Intent = new AttackOpponentIntent(enemy);
            if (enemy.HP <= tower.AttackPower)
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
            }
        }
        else
        {
            enemy.HP += (uint)tower.AttackPower;
            tower.Intent = new AttackOpponentIntent(enemy, () => { });
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (waveInProgress)
        {
            this.TickWave(delta);
        }
    }

    private void MageHit()
    {
        // TODO: Boom animation

        var mageShoots = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.mage.Position).LengthSquared())
            .Take(5)
            .Select(enemy =>
            {
                return new AttackOpponentIntent(enemy, () => enemy.GotHit(this.mage, int.MaxValue));
            })
            .ToArray();

        this.mage.Intent = new CompositeIntent<BaseUnit>(mageShoots);

        this.HeaderControl.AddBuff(nameof(SlowDown));
    }
}

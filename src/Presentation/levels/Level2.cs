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
    public FollowPathIntent(BaseLevel level)
    {
        this.level = level;
    }

    public float MoveOffset;
    private readonly BaseLevel level;

    public void Enter(BaseUnit context)
    {
    }

    public bool Execute(BaseUnit context)
    {
        context.StartMoveAnimation();
        var moveContext = (EnemyContext)context.AutomaticActionGeneratorContext;
        this.MoveOffset += context.MoveSpeed * moveContext.Delta * this.level.HeaderControl.Character.EnemySpeedCoeff;
        var pathPosition = (PathFollow2D)context.GetNode(context.PathFollow2DPath);
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
public class MoveToPointIntent : IIntent<BaseUnit>
{
    private readonly Vector2 TargetPoint;

    public MoveToPointIntent(Vector2 targetPoint)
    {
        this.TargetPoint = targetPoint;
    }
    public void Enter(BaseUnit context)
    {
    }

    public bool Execute(BaseUnit context)
    {
        context.StartMoveAnimation();
        context.UpdateAnimationDirection(this.TargetPoint - context.Position);
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

    public AttackOpponentIntent(BaseUnit opponent, Action onHit)
    {
        this.opponent = opponent;
        this.onHit = onHit;
    }

    public void Enter(BaseUnit context)
    {
    }

    public bool Execute(BaseUnit context)
    {
        if (!Godot.Object.IsInstanceValid(this.opponent))
        {
            return true;
        }

        context.StartAttackAnimation();
        context.UpdateAnimationDirection(this.opponent.Position - context.Position);
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

    private Vector2 dragonBlueInitialPosition;
    private Vector2 dragonRedInitialPosition;
    private Vector2 dragonGoldInitialPosition;

    private bool waveInProgress = false;
    private readonly Queue<BaseUnit> enemiesToSpawn = new Queue<BaseUnit>();
    private float spawnTimeout;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.SetCameraLimits(this.camera2D);

        this.dragonBlue.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { this.dragonBlue });
        this.dragonRed.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { this.dragonRed });
        this.dragonGold.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { this.dragonGold });

        // Actually towers do not have AI. They just do intents when they are set outside.
        var reasoner = new FirstScoreReasoner<BaseUnit>(1);
        reasoner.Add(new HasIntentAppraisal<BaseUnit>(1), new UseIntentAction<BaseUnit>());
        this.dragonBlue.AutomaticActionGeneratorContext = new TowerContext();
        this.dragonBlue.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.dragonBlue, reasoner);
        this.dragonRed.AutomaticActionGeneratorContext = new TowerContext();
        this.dragonRed.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.dragonRed, reasoner);
        this.dragonGold.AutomaticActionGeneratorContext = new TowerContext();
        this.dragonGold.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.dragonGold, reasoner);
        this.mage.AutomaticActionGeneratorContext = new TowerContext();
        this.mage.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.mage, reasoner);
        this.empty.AutomaticActionGeneratorContext = new TowerContext();
        this.empty.AutomaticActionGenerator = new UtilityAI<BaseUnit>(this.empty, reasoner);

        this.toBattle.Connect(CommonSignals.Pressed, this, nameof(StartWave));

        this.dragonBlueInitialPosition = this.dragonBlue.Position;
        this.dragonRedInitialPosition = this.dragonRed.Position;
        this.dragonGoldInitialPosition = this.dragonGold.Position;
    }

    private Reasoner<BaseUnit> enemyMoveReasoner;

    private void StartWave()
    {
        waveInProgress = true;

        timerLabel.ShowMessage($"Level {level + 1}.", 5);

        switch (level)
        {
            case 0:
                this.dragonBlue.Intent = new MoveToPointIntent(new Vector2(240, 740));
                this.dragonRed.Intent = new MoveToPointIntent(this.dragonRedInitialPosition);
                this.dragonGold.Intent = new MoveToPointIntent(this.dragonGoldInitialPosition);
                break;
            case 1:
                this.dragonBlue.Intent = new MoveToPointIntent(new Vector2(50, 740));
                this.dragonRed.Intent = new MoveToPointIntent(new Vector2(480 - 50, 740));
                this.dragonGold.Intent = new MoveToPointIntent(this.dragonGoldInitialPosition);
                break;
            default:
                this.dragonBlue.Intent = new MoveToPointIntent(new Vector2(50, 740));
                this.dragonRed.Intent = new MoveToPointIntent(new Vector2(480 - 50, 740));
                this.dragonGold.Intent = new MoveToPointIntent(new Vector2(240, 740));
                break;
        }

        this.mage.Intent = new MoveToPointIntent(new Vector2(255, 686));
        this.empty.Intent = new MoveToPointIntent(new Vector2(240, 400));

        var numberOfEnemies = 10 + level * 20;
        var enemyTypes = new List<string>();
        if (level >= 0)
        {
            enemyTypes.Add(nameof(SpiderBlue));
        }
        if (level >= 1)
        {
            enemyTypes.Add(nameof(SpiderRed));
        }
        if (level >= 2)
        {
            enemyTypes.Add(nameof(SpiderGold));
        }

        var startPosition = enemyPath.Curve.GetPointPosition(0);

        if (enemyMoveReasoner == null)
        {
            enemyMoveReasoner = new FirstScoreReasoner<BaseUnit>(1);
            // Intent phase
            enemyMoveReasoner.Add(new HasIntentAppraisal<BaseUnit>(1), new UseIntentAction<BaseUnit>());
            // Decision phase
            enemyMoveReasoner.Add(
                new MultAppraisal<BaseUnit>(new UnitInGroupAppraisal(Groups.AttackingEnemy), new CanAttackUnitAppraisal(this.mage)),
                new SetIntentAction<BaseUnit, AttackOpponentIntent>((c) => new AttackOpponentIntent(this.mage, () => MageHit(c.AttackPower))));
            enemyMoveReasoner.Add(
                new NotAppraisal<BaseUnit>(new CanAttackUnitAppraisal(this.mage)),
                new SetIntentAction<BaseUnit, FollowPathIntent>((c) => new FollowPathIntent(this)));
        }

        var enemies = Enumerable
            .Range(0, numberOfEnemies)
            .Select(a => BuildEnemy(enemyTypes, startPosition, enemyMoveReasoner))
            .ToList();

        enemiesToSpawn.Clear();
        spawnTimeout = float.MaxValue;
        foreach (var enemy in enemies)
        {
            enemiesToSpawn.Enqueue(enemy);
        }
        var boss = BuildEnemy(nameof(OgreGray), startPosition, enemyMoveReasoner);
        boss.Loot = new List<PackedScene> { Instantiator.LoadLoot(nameof(Gold)), Instantiator.LoadLoot(nameof(Wood)) };
        enemiesToSpawn.Enqueue(boss);
    }

    private void TickWave(float delta)
    {
        if (spawnTimeout > 0.3f / HeaderControl.Character.EnemySpeedCoeff && enemiesToSpawn.Count > 0)
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
        else if (this.GetTree().GetNodesInGroup(Groups.Enemy).Count == 0 && enemiesToSpawn.Count == 0)
        {
            level++;
            this.StartWave();
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

        this.level = 0;

        this.empty.Intent = new MoveToPointIntent(new Vector2(240, 1200));
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

        this.dragonBlue.Intent = new MoveToPointIntent(this.dragonBlueInitialPosition);
        this.dragonRed.Intent = new MoveToPointIntent(this.dragonRedInitialPosition);
        this.dragonGold.Intent = new MoveToPointIntent(this.dragonGoldInitialPosition);
    }

    private BaseUnit BuildEnemy(List<string> enemies, Vector2 position, Reasoner<BaseUnit> action)
    {
        var enemyName = enemies[r.Next(enemies.Count)];
        return BuildEnemy(enemyName, position, action);
    }

    private BaseUnit BuildEnemy(string enemyName, Vector2 position, Reasoner<BaseUnit> action)
    {
        var enemy = Instantiator.CreateUnit(enemyName);
        enemy.Position = position;
        enemy.PathFollow2DPath = this.enemyPathFollow.GetPath();
        enemy.Loot = new List<PackedScene> { Instantiator.LoadLoot(nameof(Gold)) };
        enemy.ZIndex = 1;
        enemy.AddToGroup(Groups.Enemy);
        enemy.AddToGroup(Groups.AttackingEnemy);
        enemy.AutomaticActionGeneratorContext = new EnemyContext();
        enemy.AutomaticActionGenerator = new UtilityAI<BaseUnit>(enemy, action);
        return enemy;
    }

    public async void DropLoot(BaseUnit unit)
    {
        var loots = unit.Loot.Select(loot =>
        {
            var newLoot = loot.Instance<BaseLoot>();
            newLoot.Position = unit.Position;
            this.GetParent().AddChild(newLoot);
            return newLoot;
        }).ToList();

        foreach (var newLoot in loots)
        {
            var toPosition = new Vector2(0, 0);
            var tween = newLoot.CreateTween();
            tween.TweenProperty(newLoot, "position", toPosition, 0.5f)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);
            await tween.ToMySignal(CommonSignals.Finished);
            if (this.BagInventoryPopup.TryChangeCount(newLoot.LootName, 1) == 0)
            {
                newLoot.QueueFree();
            }
        }
    }

    private readonly Dictionary<Type, HashSet<Type>> attackers = new Dictionary<Type, HashSet<Type>>
    {
        {typeof(DragonBlue), new HashSet<Type>{typeof(SpiderBlue), typeof(OgreGray)}},
        {typeof(DragonRed), new HashSet<Type>{typeof(SpiderRed), typeof(OgreGray)}},
        {typeof(DragonGold), new HashSet<Type>{typeof(SpiderGold), typeof(OgreGray)}},
    };

    private void TowerClicked(BaseUnit tower)
    {
        if (tower.Intent != null && !(tower.Intent is AttackOpponentIntent) && !(tower.Intent is CompositeIntent<BaseUnit>))
        {
            return;
        }

        var against = attackers[tower.GetType()];

        var enemy = this.GetTree()
            .GetNodesInGroup(Groups.AttackingEnemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.mage.Position).LengthSquared())
            .FirstOrDefault();

        if (enemy == null)
        {
            return;
        }

        IIntent<BaseUnit> newIntent;
        if (against.Contains(enemy.GetType()))
        {
            newIntent = new AttackOpponentIntent(enemy, () => this.EnemyHit(enemy, tower.AttackPower));
            if (enemy.HP <= tower.AttackPower)
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
            }
        }
        else
        {
            enemy.HP += (uint)tower.AttackPower;
            newIntent = new AttackOpponentIntent(enemy, () => { });
        }

        if (tower.Intent == null)
        {
            tower.Intent = newIntent;
        }
        else
        {
            tower.Intent = new CompositeIntent<BaseUnit>(tower.Intent, newIntent);

        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (waveInProgress)
        {
            if (Input.IsActionJustPressed("blue") && level >= 0)
            {
                TowerClicked(this.dragonBlue);
            }
            if (Input.IsActionJustPressed("red") && level >= 1)
            {
                TowerClicked(this.dragonRed);
            }
            if (Input.IsActionJustPressed("gold") && level >= 2)
            {
                TowerClicked(this.dragonGold);
            }
            this.TickWave(delta);
        }
    }

    private void EnemyHit(BaseUnit enemy, int attackPower)
    {
        if (!Godot.Object.IsInstanceValid(enemy))
        {
            return;
        }

        enemy.HP -= (uint)Math.Min(attackPower, enemy.HP);
        if (enemy.HP <= 0)
        {
            this.DropLoot(enemy);
            enemy.QueueFree();
        }
    }

    private void MageHit(int attackPower)
    {
        // TODO: Boom animation

        this.mage.HP -= Math.Min(this.mage.HP, (uint)attackPower);

        var mageShoots = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.mage.Position).LengthSquared())
            .Take(5)
            .Select(enemy =>
            {
                return new AttackOpponentIntent(enemy, () => this.EnemyHit(enemy, this.mage.AttackPower));
            })
            .ToArray();

        this.mage.Intent = new CompositeIntent<BaseUnit>(mageShoots);

        this.HeaderControl.AddBuff(nameof(SlowDown));
    }
}

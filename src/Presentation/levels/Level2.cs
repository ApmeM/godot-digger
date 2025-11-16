using System;
using System.Collections.Generic;
using System.Linq;
using BrainAI.AI.UtilityAI;
using Godot;

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
public class HasCurrentActionAppraisal : IAppraisal<BaseUnit>
{
    private readonly float score;

    public HasCurrentActionAppraisal(float score)
    {
        this.score = score;
    }

    public float GetScore(BaseUnit context)
    {
        if (context.CurrentAction != null)
        {
            return score;
        }

        return 0f;
    }
}
public class DoCurrentActionAction : IAction<BaseUnit>
{
    public void Enter(BaseUnit context)
    {
        ((IIntent<BaseUnit>)context.CurrentAction).Enter(context);
    }

    public void Execute(BaseUnit context)
    {
        if (((IIntent<BaseUnit>)context.CurrentAction).Execute(context))
        {
            Exit(context);
        }
    }

    public void Exit(BaseUnit context)
    {
        ((IIntent<BaseUnit>)context.CurrentAction)?.Exit(context);
        context.CurrentAction = null;
    }
}
public class SetCurrentActionAction : IAction<BaseUnit>
{
    private readonly Func<BaseUnit, Resource> intentFactory;

    public SetCurrentActionAction(Resource intent)
    {
        intentFactory = (BaseUnit ctx) => intent;
    }

    public SetCurrentActionAction(Func<BaseUnit, Resource> intentFactory)
    {
        this.intentFactory = intentFactory;
    }

    public void Enter(BaseUnit context)
    {
    }

    public void Execute(BaseUnit context)
    {
        context.CurrentAction = intentFactory(context);
    }

    public void Exit(BaseUnit context)
    {
    }
}

[SceneReference("Level2.tscn")]
public partial class Level2
{
    private Random r = new Random();
    private int level = 0;

    private BaseUnit[] dragons = new BaseUnit[3];

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

        dragons[0] = dragonBlue;
        dragons[1] = dragonRed;
        dragons[2] = dragonGold;

        // Actually towers do not have AI. They just do intents when they are set outside.
        foreach (var dragon in dragons)
        {
            dragon.Connect(nameof(BaseUnit.Clicked), this, nameof(TowerClicked), new Godot.Collections.Array { dragon });
        }

        this.toBattle.Connect(CommonSignals.Pressed, this, nameof(ToBattleClicked));

        this.dragonBlueInitialPosition = this.dragonBlue.Position;
        this.dragonRedInitialPosition = this.dragonRed.Position;
        this.dragonGoldInitialPosition = this.dragonGold.Position;

        this.header.TrackingUnit = this.mage;

        this.cocoon.Connect(nameof(BaseUnit.Clicked), this, nameof(CocoonClicked), new Godot.Collections.Array { this.cocoon });
        this.cocoon.Connect(nameof(BaseUnit.OnHit), this, nameof(CocoonHit));

        this.mage.Connect(nameof(BaseUnit.OnHit), this, nameof(MageHit));
    }

    private void CocoonHit(BaseUnit attacker, BaseUnit defender)
    {
        defender.QueueFree();

        var talk = new List<QuestPopupData>
        {
            new QuestPopupData { Description = " Thank you very much! \n Our village was attacked by giant spiders. \n They left for now, but I think they will return." },
            new QuestPopupData { Description = " Save us please, go to the main gate and stop the invasion. \n Find me in the village and I'll check what can I do for you." }
        };

        this.leader.Connect(nameof(BaseUnit.Clicked), this, nameof(QuestClicked), new Godot.Collections.Array { this.leader });
        this.leader.Visible = true;
        this.leader.CurrentAction = new CustomActionQueue
        {
            CustomActions = new List<Resource>
                    {
                        new CustomActionTalk { QuestData = talk, QuestPopupPath = this.questPopup.GetPath() },
                        new CustomActionMoveToPoint { TargetPoint = new Vector2(241, 1215) },
                        new CustomActionMoveToPoint { TargetPoint = new Vector2(337, 1265) }
                    }
        };
    }

    private void CocoonClicked(BaseUnit cocoon)
    {
        this.mage.CurrentAction = new CustomActionQueue
        {
            CustomActions = new List<Resource> {
                new CustomActionMoveToPoint { TargetPoint = cocoon.Position + Vector2.Left * 40 },
                new CustomActionAttackOpponent { OpponentPath = cocoon.GetPath() }
            }
        };
    }

    private void QuestClicked(BaseUnit unit)
    {
        var talk = new List<QuestPopupData>
        {
            new QuestPopupData { Description = "Hi again." },
            new QuestPopupData
            {
                Description = "Please bring me a few wooden sticks.",
                Requirements = new List<QuestData>
                    {
                        new QuestData{
                            Loot = Instantiator.LoadLoot(nameof(Wood)),
                            Count = 2
                        }
                    },
                Rewards = new List<QuestData>
                    {
                        new QuestData{
                            Loot = Instantiator.LoadLoot(nameof(Gold)),
                            Count = 10
                        }
                    }
            }
        };

        this.mage.CurrentAction = new CustomActionQueue
        {
            CustomActions = new List<Resource>{
                new CustomActionMoveToPoint
                {
                    TargetPoint = unit.Position + Vector2.Left * 40
                },
                new CustomActionTalk
                {
                    InventoryUnitPath = this.mage.GetPath(),
                    QuestData = talk,
                    QuestPopupPath = this.questPopup.GetPath(),
                }
            }
        };
    }

    private void ToBattleClicked()
    {
        this.mage.CurrentAction = new CustomActionMoveToPoint
        {
            TargetPoint = new Vector2(255, 686)
        };
        this.bat.CurrentAction = new CustomActionFollowPath
        {
            MagePath = this.mage.GetPath(),
            FollowPath = this.batPathForwardFollow.GetPath(),
            MoveOffset = 0,
        };

        StartWave();
    }

    private void StartWave()
    {
        waveInProgress = true;

        timerLabel.ShowMessage($"Level {level + 1}.", 5);

        switch (level)
        {
            case 0:
                this.dragonBlue.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(240, 740) };
                this.dragonRed.CurrentAction = new CustomActionMoveToPoint { TargetPoint = this.dragonRedInitialPosition };
                this.dragonGold.CurrentAction = new CustomActionMoveToPoint { TargetPoint = this.dragonGoldInitialPosition };
                break;
            case 1:
                this.dragonBlue.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(50, 740) };
                this.dragonRed.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(GetViewport().Size.x - 50, 740) };
                this.dragonGold.CurrentAction = new CustomActionMoveToPoint { TargetPoint = this.dragonGoldInitialPosition };
                break;
            default:
                this.dragonBlue.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(50, 740) };
                this.dragonRed.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(GetViewport().Size.x - 50, 740) };
                this.dragonGold.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(GetViewport().Size.x / 2, 740) };
                break;
        }

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

        var enemies = Enumerable
            .Range(0, numberOfEnemies)
            .Select(a => BuildEnemy(enemyTypes, startPosition))
            .ToList();

        enemiesToSpawn.Clear();
        spawnTimeout = float.MaxValue;
        foreach (var enemy in enemies)
        {
            enemiesToSpawn.Enqueue(enemy);
        }
        var boss = BuildEnemy(nameof(OgreGray), startPosition);
        boss.Inventory.TryChangeCount(nameof(Wood), 1);
        enemiesToSpawn.Enqueue(boss);
    }

    private void TickWave(float delta)
    {
        if (this.GetTree().Paused)
        {
            return;
        }

        if (spawnTimeout > 0.5f / this.header.TrackingUnit.EnemySpeedCoeff && enemiesToSpawn.Count > 0)
        {
            spawnTimeout = 0;
            var enemy = enemiesToSpawn.Dequeue();
            this.floor.AddChild(enemy);
        }

        spawnTimeout += delta;

        if (this.mage.HP < 1)
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

        this.bat.CurrentAction = new CustomActionFollowPath
        {
            MagePath = this.mage.GetPath(),
            FollowPath = this.batPathBackFollow.GetPath(),
            MoveOffset = 0,
        };
        this.mage.CurrentAction = new CustomActionMoveToPoint { TargetPoint = new Vector2(297, 1004) };
        this.mage.HP = this.mage.MaxHP;

        var enemies = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .ToList();
        foreach (var enemy in enemies)
        {
            enemy.QueueFree();
        }

        this.mage.Buffs.Clear();
        this.header.UpdateTrackingUnit();

        this.dragonBlue.CurrentAction = new CustomActionMoveToPoint { TargetPoint = this.dragonBlueInitialPosition };
        this.dragonRed.CurrentAction = new CustomActionMoveToPoint { TargetPoint = this.dragonRedInitialPosition };
        this.dragonGold.CurrentAction = new CustomActionMoveToPoint { TargetPoint = this.dragonGoldInitialPosition };
    }

    private BaseUnit BuildEnemy(List<string> enemies, Vector2 position)
    {
        var enemyName = enemies[r.Next(enemies.Count)];
        return BuildEnemy(enemyName, position);
    }

    private BaseUnit BuildEnemy(string enemyName, Vector2 position)
    {
        var enemy = Instantiator.CreateUnit(enemyName);
        enemy.Position = position;

        enemy.InitialSlotsCount = 2;
        enemy.Inventory.TryChangeCount(nameof(Gold), 1);
        enemy.ZIndex = 1;
        enemy.AddToGroup(Groups.Enemy);
        enemy.AddToGroup(Groups.AttackingEnemy);
        enemy.CurrentAction = new CustomActionQueue
        {
            CustomActions = new List<Resource>
            {
                new CustomActionFollowPath{
                    MagePath = this.mage.GetPath(),
                    FollowPath = this.enemyPathFollow.GetPath(),
                },
                new CustomActionAttackOpponent{
                    OpponentPath = this.mage.GetPath()
                }
            }
        };

        enemy.Connect(nameof(BaseUnit.OnHit), this, nameof(EnemyHit));
        return enemy;
    }

    public async void DropLoot(BaseUnit unit)
    {
        var loots = unit.Inventory.Inventory.Slots
            .Union(new[] { unit.Inventory.Money })
            .Where(a => a.HasItem())
            .Select(loot =>
        {
            var newLoot = Instantiator.CreateLoot(loot.LootName);
            newLoot.Position = unit.Position;
            this.GetParent().AddChild(newLoot);
            this.mage.Inventory.TryChangeCount(newLoot.LootName, 1);
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

            newLoot.QueueFree();
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
        if (tower.CurrentAction != null && !(tower.CurrentAction is CustomActionAttackOpponent) && !(tower.CurrentAction is CustomActionAllAtOnce))
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

        Resource newIntent;
        if (against.Contains(enemy.GetType()))
        {
            if (enemy.HP <= tower.AttackPower)
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
            }
        }
        else
        {
            enemy.HP += (uint)tower.AttackPower;
        }

        newIntent = new CustomActionAttackOpponent { OpponentPath = enemy.GetPath() };

        if (tower.CurrentAction == null)
        {
            tower.CurrentAction = newIntent;
        }
        else
        {
            tower.CurrentAction = new CustomActionAllAtOnce
            {
                CustomActions = new List<Resource>
                {
                    tower.CurrentAction,
                    newIntent
                }
            };
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

    private void EnemyHit(BaseUnit attacker, BaseUnit defender)
    {
        if (!Godot.Object.IsInstanceValid(defender))
        {
            return;
        }

        if (this.dragons.Contains(attacker) && !attackers[attacker.GetType()].Contains(defender.GetType()))
        {
            return;
        }

        defender.HP -= (uint)Math.Min(attacker.Character.AttackPower, defender.HP);
        if (defender.HP <= 0)
        {
            this.DropLoot(defender);
            defender.QueueFree();
        }
    }

    private void MageHit(BaseUnit attacker, BaseUnit defender)
    {
        if (defender != this.mage)
        {
            throw new InvalidOperationException();
        }

        var attackPower = attacker.Character.AttackPower;

        // TODO: Boom animation
        this.mage.HP -= Math.Min(this.mage.HP, (uint)attackPower);

        var mageShoots = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.mage.Position).LengthSquared())
            .Take(5)
            .Select(enemy =>
            {
                if (enemy.HP <= this.mage.Character.AttackPower)
                {
                    enemy.RemoveFromGroup(Groups.AttackingEnemy);
                }
                return new CustomActionAttackOpponent { OpponentPath = enemy.GetPath() };
            })
            .Cast<Resource>()
            .ToList();

        this.mage.CurrentAction = new CustomActionAllAtOnce { CustomActions = mageShoots };
        this.mage.Buffs.AddBuff(nameof(SlowDown));
        this.header.UpdateTrackingUnit();
    }
}

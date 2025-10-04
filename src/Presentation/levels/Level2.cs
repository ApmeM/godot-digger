using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

[SceneReference("Level2.tscn")]
public partial class Level2
{
    private Random r = new Random();
    private int level = 0;

    private Vector2 leftTowerInitialPosition;
    private Vector2 rightTowerInitialPosition;
    private Vector2 centerTowerInitialPosition;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.mage.Connect(nameof(BaseUnit.OnHit), this, nameof(MageHit));
        this.leftTower.Connect(nameof(BaseUnit.Clicked), this, nameof(LeftTowerClicked));
        this.rightTower.Connect(nameof(BaseUnit.Clicked), this, nameof(RightTowerClicked));
        this.centerTower.Connect(nameof(BaseUnit.Clicked), this, nameof(CenterTowerClicked));

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

    private async void StartWave()
    {
        timerLabel.ShowMessage($"Level {level + 1}.", 5);

        switch (level)
        {
            case 0:

                this.MoveUnit(this.leftTower, new Vector2(240, 740));
                this.MoveUnit(this.rightTower, this.rightTowerInitialPosition);
                this.MoveUnit(this.centerTower, this.centerTowerInitialPosition);
                break;
            case 1:
                this.MoveUnit(this.leftTower, new Vector2(50, 740));
                this.MoveUnit(this.rightTower, new Vector2(480 - 50, 740));
                this.MoveUnit(this.centerTower, this.centerTowerInitialPosition);
                break;
            default:
                this.MoveUnit(this.leftTower, new Vector2(50, 740));
                this.MoveUnit(this.rightTower, new Vector2(480 - 50, 740));
                this.MoveUnit(this.centerTower, new Vector2(240, 740));
                break;
        }

        this.MoveUnit(this.mage, new Vector2(255, 686));

        var tween = this.CreateTween();
        tween.TweenProperty(this.camera2D, "position", new Vector2(240, 400), 2)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        await tween.ToMySignal(CommonSignals.Finished);

        var numberOfEnemies = 10 + level * 2;
        var enemies = new List<string>();
        if (level >= 0)
        {
            enemies.Add(nameof(Wolf));
        }
        if (level >= 1)
        {
            enemies.Add(nameof(Wasp));
        }
        if (level >= 2)
        {
            enemies.Add(nameof(Slime));
        }

        var speed = 150 + level * 15;
        var path = enemyPath.Curve.GetBakedPoints().Reverse().ToArray();

        for (var i = 0; i < numberOfEnemies - 1; i++)
        {
            var enemy = BuildEnemy(enemies, path, speed);
            await this.GetTree().CreateTimer(0.3f).ToMySignal(CommonSignals.Timeout);
        }

        var lastEnemy = BuildEnemy(enemies, path, speed);
        await lastEnemy.ToMySignal(nameof(BaseUnit.LootDropped));

        this.StopWave();
        level++;
    }

    private async void StopWave()
    {
        if (this.mage.HP <= 1)
        {
            timerLabel.ShowMessage($"Game Over.", 5);
        }
        else
        {
            timerLabel.ShowMessage($"Level clear.", 5);

            this.MoveUnit(this.mage, new Vector2(297, 1004));
        }
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

        this.MoveUnit(this.leftTower, this.leftTowerInitialPosition);
        this.MoveUnit(this.rightTower, this.rightTowerInitialPosition);
        this.MoveUnit(this.centerTower, this.centerTowerInitialPosition);

        var tween = this.CreateTween();
        tween.TweenProperty(this.camera2D, "position", new Vector2(240, 1000), 2)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        await tween.ToMySignal(CommonSignals.Finished);
    }

    private async void MoveUnit(BaseUnit tower, Vector2 direction)
    {
        await tower.StartMoveAction(direction);
        await tower.StartStayAction();
    }

    private BaseUnit BuildEnemy(List<string> enemies, Vector2[] path, int speed)
    {
        var enemyName = enemies[r.Next(enemies.Count)];
        var enemy = Instantiator.CreateUnit(enemyName);
        this.floor.AddChild(enemy);
        enemy.Position = path[0];
        enemy.LevelPath = this.GetPath();
        enemy.VisionDistance = 1000;
        enemy.AggroAgainst = new List<string> { "grp_player" };
        enemy.AttackPower = 1;
        enemy.AttackDistance = 100;
        enemy.Loot = new List<PackedScene> { Instantiator.LoadLoot(nameof(Gold)) };
        enemy.MaxHP = 3;
        enemy.HP = 1;
        enemy.MoveSpeed = speed;
        enemy.ZIndex = 1;
        enemy.HitDelay = enemy.AttackDelay / 2;
        enemy.AddToGroup(Groups.Enemy);
        enemy.AddToGroup(Groups.AttackingEnemy);
        enemy.Connect(nameof(BaseUnit.LootDropped), this, nameof(LootDropped));
        enemy.AutomaticActionGenerator = new MoverToFirstFound(enemy, this, new MoverToConstant(enemy, this, path), new AttackMove(enemy, this), new StandStil(enemy, this));
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

        if (!Godot.Object.IsInstanceValid(this))
        {
            // Either attacker or defender no longer on a scene. 
            // No need to calculate attcks.
            return;
        }
        newLoot.LootClicked();
    }

    private void RightTowerClicked()
    {
        TowerClicked(this.rightTower, typeof(Wasp));
    }

    private void LeftTowerClicked()
    {
        TowerClicked(this.leftTower, typeof(Wolf));
    }

    private void CenterTowerClicked()
    {
        TowerClicked(this.centerTower, typeof(Slime));
    }

    private void TowerClicked(BaseUnit tower, Type against)
    {
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
            tower.StartAttackAction(enemy, () => enemy.GotHit(tower, tower.AttackPower));
            if (enemy.HP <= tower.AttackPower)
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
            }
        }
        else
        {
            enemy.HP += (uint)tower.AttackPower;
            tower.StartAttackAction(enemy, () => { });
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    private async void MageHit(int hpLeft)
    {
        // TODO: Boom animation

        var mageShoots = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.mage.Position).LengthSquared())
            .Take(5)
            .Select(enemy =>
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
                return this.mage.StartAttackAction(enemy, () => enemy.GotHit(this.mage, int.MaxValue));
            })
            .ToArray();

        this.HeaderControl.AddBuff(nameof(SlowDown));

        await Task.WhenAll(mageShoots);

        if (hpLeft <= 1)
        {
            this.StopWave();
        }
    }
}

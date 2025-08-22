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

        this.door.Connect(nameof(BaseUnit.OnHit), this, nameof(DoorHit));
        this.leftTower.Connect(nameof(BaseUnit.Clicked), this, nameof(LeftTowerClicked));
        this.rightTower.Connect(nameof(BaseUnit.Clicked), this, nameof(RightTowerClicked));
        this.centerTower.Connect(nameof(BaseUnit.Clicked), this, nameof(CenterTowerClicked));

        this.toBattle.Connect(CommonSignals.Pressed, this, nameof(StartWave));
        this.upgradeDoor.Connect(CommonSignals.Pressed, this, nameof(UpgradeDoor));

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

    private void UpgradeDoor()
    {
        var count = this.BagInventoryPopup.GetItemCount(nameof(Gold));
        if (count > this.door.MaxHP * this.door.MaxHP)
        {
            this.BagInventoryPopup.TryChangeCount(nameof(Gold), -(int)(this.door.MaxHP * this.door.MaxHP));
            this.door.MaxHP++;
            this.door.HP = this.door.MaxHP;
        }
    }

    private async void StartWave()
    {
        timerLabel.ShowMessage($"Level {level + 1}.", 5);

        this.leftTower.CancelAction();
        this.rightTower.CancelAction();
        this.centerTower.CancelAction();
        switch (level)
        {
            case 0:
                this.leftTower.StartMoveAction(new Vector2(240, 740));
                this.rightTower.StartMoveAction(this.rightTowerInitialPosition);
                this.centerTower.StartMoveAction(this.centerTowerInitialPosition);
                break;
            case 1:
                this.leftTower.StartMoveAction(new Vector2(50, 740));
                this.rightTower.StartMoveAction(new Vector2(480 - 50, 740));
                this.centerTower.StartMoveAction(this.centerTowerInitialPosition);
                break;
            default:
                this.leftTower.StartMoveAction(new Vector2(50, 740));
                this.rightTower.StartMoveAction(new Vector2(480 - 50, 740));
                this.centerTower.StartMoveAction(new Vector2(240, 740));
                break;
        }

        var tween = this.CreateTween();
        tween.TweenProperty(this.camera2D, "position", new Vector2(240, 400), 2)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        await this.ToSignal(tween, CommonSignals.Finished);

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
            await this.ToSignal(this.GetTree().CreateTimer(0.3f), CommonSignals.Timeout);
        }

        var lastEnemy = BuildEnemy(enemies, path, speed);
        await ToSignal(lastEnemy, nameof(BaseUnit.LootDropped));

        this.StopWave();
        level++;
    }

    private async void StopWave()
    {
        if (this.door.HP <= 1)
        {
            timerLabel.ShowMessage($"Game Over.", 5);
        }
        else
        {
            timerLabel.ShowMessage($"Level clear.", 5);
        }
        this.door.HP = this.door.MaxHP;

        var enemies = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .ToList();
        foreach (var enemy in enemies)
        {
            enemy.QueueFree();
        }

        this.leftTower.CancelAction();
        this.rightTower.CancelAction();
        this.centerTower.CancelAction();
        this.leftTower.StartMoveAction(this.leftTowerInitialPosition);
        this.rightTower.StartMoveAction(this.rightTowerInitialPosition);
        this.centerTower.StartMoveAction(this.centerTowerInitialPosition);

        var tween = this.CreateTween();
        tween.TweenProperty(this.camera2D, "position", new Vector2(240, 1000), 2)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        await this.ToSignal(tween, CommonSignals.Finished);
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
        enemy.Scale = new Vector2(1.5f, 1.5f);
        enemy.HitDelay = enemy.AttackDelay / 2;
        enemy.AddToGroup(Groups.Enemy);
        enemy.AddToGroup(Groups.AttackingEnemy);
        enemy.Connect(nameof(BaseUnit.LootDropped), this, nameof(LootDropped));
        enemy.AutomaticActionGenerator = new MoverToFirstFound(enemy, this, new MoverToConstant(enemy, this, path), new AttackMove(enemy, this));
        return enemy;
    }

    private async void LootDropped(BaseLoot newLoot)
    {
        var toPosition = new Vector2(0, 0);
        var tween = newLoot.CreateTween();
        tween.TweenProperty(newLoot, "position", toPosition, 0.5f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
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
            .OrderBy(a => (a.Position - this.door.Position).LengthSquared())
            .FirstOrDefault();

        if (enemy == null)
        {
            return;
        }

        tower.CancelAction();

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
            enemy.HP+= (uint)tower.AttackPower; 
            tower.StartAttackAction(enemy, () => { });
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    private void DoorHit(int hpLeft)
    {
        // TODO: Boom animation

        var enemies = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.door.Position).LengthSquared())
            .ToList();

        var i = 0;
        foreach (var enemy in enemies)
        {
            if (i < 5)
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
                enemy.GotHit(this.door, int.MaxValue);
            }
            else
            {
                enemy.MoveSpeed /= 2;
            }
            i++;
        }

        if (hpLeft <= 1)
        {
            this.StopWave();
        }
    }
}

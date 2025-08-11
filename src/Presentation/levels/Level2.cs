using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

[SceneReference("Level2.tscn")]
public partial class Level2
{
    private Random r = new Random();
    private int level;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.door.Connect(nameof(BaseUnit.OnHit), this, nameof(DoorHit));
        this.leftTower.Connect(nameof(BaseUnit.Clicked), this, nameof(LeftTowerClicked));
        this.rightTower.Connect(nameof(BaseUnit.Clicked), this, nameof(RightTowerClicked));
        this.centerTower.Connect(nameof(BaseUnit.Clicked), this, nameof(CenterTowerClicked));

        BuildEnemies();

        this.leftTower.AutomaticPathGenerator = null;
        this.rightTower.AutomaticPathGenerator = null;
        this.centerTower.AutomaticPathGenerator = null;

        this.leftTower.AttackDelay = 0.3f;
        this.leftTower.HitDelay = 0.1f;
        this.rightTower.AttackDelay = 0.3f;
        this.rightTower.HitDelay = 0.1f;
        this.centerTower.AttackDelay = 0.3f;
        this.centerTower.HitDelay = 0.1f;
    }

    public void BuildEnemies()
    {
        var numberOfEnemies = 10 + level * 2;
        var enemies = new List<string>();
        if (level == 0)
        {
            this.leftTower.StartMoveAction(new Vector2(240, 740));
        }
        if (level >= 0)
        {
            enemies.Add(nameof(Wolf));
        }

        if (level == 1)
        {
            this.leftTower.CancelAction();
            this.leftTower.StartMoveAction(new Vector2(50, 740));
            this.rightTower.StartMoveAction(new Vector2(480 - 50, 740));
        }
        if (level >= 1)
        {
            enemies.Add(nameof(Wasp));
        }

        if (level == 2)
        {
            this.centerTower.StartMoveAction(new Vector2(240, 740));
        }
        if (level >= 2)
        {
            enemies.Add(nameof(Slime));
        }

        var speed = 150 + level * 15;

        for (var i = 0; i < numberOfEnemies; i++)
        {
            var enemyName = enemies[r.Next(enemies.Count)];
            var enemy = Instantiator.CreateUnit(enemyName);
            this.floor.AddChild(enemy);
            this.pathFollow2D.UnitOffset = 1f * i / numberOfEnemies;
            enemy.Position = this.pathFollow2D.Position;
            enemy.LevelPath = this.GetPath();
            enemy.VisionDistance = 1000;
            enemy.AggroAgainst = new List<string> { "grp_player" };
            enemy.AttackPower = 1;
            enemy.AttackDistance = 100;
            enemy.Loot = new List<PackedScene>();
            enemy.MaxHP = 5;
            enemy.HP = 1;
            enemy.MoveSpeed = speed;
            enemy.ZIndex = 1;
            enemy.Scale = new Vector2(1.5f, 1.5f);
            enemy.HitDelay = enemy.AttackDelay / 2;
            enemy.AddToGroup(Groups.Enemy);
            enemy.AddToGroup(Groups.AttackingEnemy);
        }
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

        if (against == enemy.GetType())
        {
            tower.CancelAction();
            tower.StartAttackAction(enemy, () => enemy.GotHit(tower, tower.AttackPower));
            if (enemy.HP <= tower.AttackPower)
            {
                enemy.RemoveFromGroup(Groups.AttackingEnemy);
            }
        }
        else
        {
            enemy.HP += (uint)tower.AttackPower;
            tower.CancelAction();
            tower.StartAttackAction(enemy, () =>
            {
                if (!Godot.Object.IsInstanceValid(enemy))
                {
                    return;
                }
                enemy.HP -= (uint)tower.AttackPower;
                enemy.GotHit(tower, -tower.AttackPower);
            });
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        var enemy = this.GetTree().GetFirstNodeInGroup(Groups.Enemy);
        if (enemy == null)
        {
            // Next level.
            this.level++;
            this.BuildEnemies();
        }
    }

    private void DoorHit(int hpLeft)
    {
        // TODO: Boom animation

        var enemies = this.GetTree()
            .GetNodesInGroup(Groups.Enemy)
            .Cast<BaseUnit>()
            .ToList();

        foreach (var enemy in enemies)
        {
            enemy.GotHit(this.door, int.MaxValue);
        }

        if (hpLeft <= 0)
        {
            this.EmitSignal(nameof(GameOver));
        }
        else
        {
            level--;
        }
    }
}

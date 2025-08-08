using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Level2.tscn")]
public partial class Level2
{
    private Random r = new Random();
    private int level;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.door.Connect(CommonSignals.TreeExited, this, nameof(DoorTreeExit));
        this.dragon.Connect(nameof(BaseUnit.Clicked), this, nameof(LeftTowerClicked));
        this.dragon2.Connect(nameof(BaseUnit.Clicked), this, nameof(RightTowerClicked));

        BuildEnemies();
    }

    public void BuildEnemies()
    {
        var numberOfEnemies = 10 + level * 2;
        var enemies = new List<string> { nameof(Wolf), nameof(Wasp) };
        var speed = 100 + level * 10;

        for (var i = 0; i < numberOfEnemies; i++)
        {
            var enemyName = enemies[r.Next(enemies.Count)];
            var enemy = Instantiator.CreateUnit(enemyName);
            this.floor.AddChild(enemy);
            this.pathFollow2D.UnitOffset = 1f * i / numberOfEnemies;
            enemy.Position = this.pathFollow2D.Position;
            enemy.LevelPath = this.GetPath();
            enemy.VisionDistance = 100;
            enemy.AggroAgainst = new List<string> { "grp_player" };
            enemy.AttackPower = 1;
            enemy.AddToGroup("grp_enemy");
            enemy.Loot = new List<PackedScene>();
            enemy.MaxHP = 5;
            enemy.HP = 1;
            enemy.MoveSpeed = speed;
        }
    }

    private void RightTowerClicked()
    {
        var enemy = this.GetTree()
            .GetNodesInGroup("grp_enemy")
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.door.Position).LengthSquared())
            .FirstOrDefault();

        if (enemy == null)
        {
            return;
        }

        if (typeof(Wasp) == enemy.GetType())
        {
            enemy.GotHit(null, this.dragon.AttackPower);
        }
        else if (typeof(Wolf) == enemy.GetType())
        {
            enemy.HP += (uint)this.dragon.AttackPower;
        }
    }

    private void LeftTowerClicked()
    {
        var enemy = this.GetTree()
            .GetNodesInGroup("grp_enemy")
            .Cast<BaseUnit>()
            .OrderBy(a => (a.Position - this.door.Position).LengthSquared())
            .FirstOrDefault();

        if (enemy == null)
        {
            return;
        }

        if (typeof(Wasp) == enemy.GetType())
        {
            enemy.HP += (uint)this.dragon.AttackPower;
        }
        else if (typeof(Wolf) == enemy.GetType())
        {
            enemy.GotHit(null, this.dragon.AttackPower);
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        var enemy = this.GetTree().GetFirstNodeInGroup("grp_enemy");
        if (enemy == null)
        {
            // Next level.
            this.level++;
            this.BuildEnemies();
        }
    }

    private void DoorTreeExit()
    {
        this.EmitSignal(nameof(GameOver));
    }
}

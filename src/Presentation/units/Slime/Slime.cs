using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Slime.tscn")]
public partial class Slime
{
    public Slime()
    {
        this.HP = 2;
        this.AttackPower = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }

    [Export]
    public float Speed = 100;

    [Export]
    public float MoveDelay = 1;

    private Vector2? path;

    private float currentMoveDelay;

    public override void _Process(float delta)
    {
        base._Process(delta);

        currentMoveDelay += delta;
        if (currentMoveDelay <= MoveDelay)
        {
            return;
        }

        var level = this.GetNode<BaseLevel>(this.LevelPath);
        if (path == null)
        {
            var floors = new HashSet<Floor> { Floor.Ground, Floor.Tiles };
            this.path = this.GetPathToLoot(floors) ?? this.GetPathToRandomLocation(floors);
            if (this.path == null)
            {
                path = null;
                currentMoveDelay = 0;
                return;
            }
            path = level.FloorMap.MapToWorld(path.Value);
        }

        if (base.MoveUnit(path.Value, Speed * delta))
        {
            var loots = this.GetTree()
                .GetNodesInGroup(Groups.Loot)
                .Cast<BaseLoot>()
                .Where(a => level.FloorMap.WorldToMap(a.RectPosition) == level.FloorMap.WorldToMap(this.RectPosition))
                .ToList();

            foreach (var l in loots)
            {
                this.Loot.Add(l.LootName);
                l.QueueFree();
            }

            path = null;
            currentMoveDelay = 0;
            return;
        }
    }


    public override void GotHit(BaseUnit from, int attackPower)
    {
        base.GotHit(from, attackPower);
        if (this.HP <= 0)
        {
            var level = this.GetNode<BaseLevel>(this.LevelPath);
            level.FloatingTextManagerControl.ShowValue(Instantiator.CreateBuff(Buff.Dead), this.RectPosition);
        }
    }
}

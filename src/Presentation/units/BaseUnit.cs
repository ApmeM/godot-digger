using System;
using System.Collections.Generic;
using System.Linq;
using BrainAI.Pathfinding;
using Godot;

[SceneReference("BaseUnit.tscn")]
public partial class BaseUnit
{
    public uint HP;
    public int AttackPower;
    public List<string> Loot = new List<string>();

    public HashSet<string> AggroAgainst = new HashSet<string>();

    private static Random random = new Random();

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

    private IPathfinder<(Vector2, HashSet<Floor>)> internalPathfinder;
    protected IPathfinder<(Vector2, HashSet<Floor>)> pathfinder
    {
        get
        {
            internalPathfinder = internalPathfinder ?? new AStarPathfinder<(Vector2, HashSet<Floor>)>(level);
            return internalPathfinder;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Unit);
    }

    public void UnitClicked()
    {
        if (!level.HeaderControl.Character.CanDig)
        {
            return;
        }

        var worldPos = this.Position;

        if (level.HeaderControl.CurrentStamina == 0)
        {
            level.FloatingTextManagerControl.ShowValue("Too tired", worldPos, new Color(0.60f, 0.85f, 0.91f));
            return;
        }

        level.HeaderControl.CurrentStamina--;
        level.FloatingTextManagerControl.ShowValue((-1).ToString(), worldPos, new Color(0.60f, 0.85f, 0.91f));

        this.GotHit(null, (int)level.HeaderControl.Character.DigPower);

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
            var newLoot = Instantiator.CreateLoot(loot);
            newLoot.LevelPath = this.LevelPath;
            newLoot.Position = this.Position;
            this.GetParent().AddChild(newLoot);
        }
    }

    protected Vector2? GetPathToRandomLocation(HashSet<Floor> floors)
    {
        var pos = level.WorldToMap(this.Position);
        var dest = pos + level.moveDirections[random.Next(level.moveDirections.Length)];
        pathfinder.Search((pos, floors), (dest, floors));
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path[1].Item1;
    }

    protected Vector2? GetPathToLoot(HashSet<Floor> floors)
    {
        var pos = level.WorldToMap(this.Position);

        var loots = this.GetTree()
            .GetNodesInGroup(Groups.Loot)
            .Cast<BaseLoot>()
            .Select(a => (level.WorldToMap(a.Position), floors))
            .ToHashSet();

        pathfinder.Search((pos, floors), loots);
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path[1].Item1;
    }

    protected Vector2? GetPathToOtherGroup(HashSet<Floor> floors)
    {
        var groupsToAttack = this.AggroAgainst.Except(this.GetGroups().Cast<string>()).ToArray();

        var pos = level.WorldToMap(this.Position);
        var otherGroups = groupsToAttack
            .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Select(a => (level.WorldToMap(a.Position), floors))
            .ToHashSet();

        pathfinder.Search((pos, floors), otherGroups);
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path[1].Item1;
    }

    protected bool MoveUnit(Vector2 destination, float speed)
    {
        var direction = destination - this.Position;
        if (direction.LengthSquared() <= speed * speed)
        {
            this.Position = destination;
            return true;
        }

        this.Position += direction.Normalized() * speed;
        return false;
    }

    public virtual void GotHit(BaseUnit from, int attackPower)
    {
        var hitPower = (uint)Math.Min(attackPower, this.HP);
        this.HP -= hitPower;
        level.FloatingTextManagerControl.ShowValue((-hitPower).ToString(), this.Position, new Color(1, 0, 0));

        if (from != null)
        {
            var enemyGroups = Groups.GroupsListForAggro.Intersect(from.GetGroups().Cast<string>()).ToArray();
            var myGroups = Groups.GroupsListForAggro
                .Intersect(this.GetGroups().Cast<string>())
                .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
                .ToHashSet();

            foreach (var unit in myGroups)
            {
                foreach (var enemy in enemyGroups)
                {
                    unit.AggroAgainst.Add(enemy);
                }
            }
        }

        if (this.HP <= 0)
        {
            this.DropLoot();
            this.QueueFree();
        }
    }

    internal bool TryAttackAt(Vector2 at)
    {
        if ((this.Position - at).LengthSquared() >= 48 * 48)
        {
            return false;
        }

        var groupsToAttack = this.AggroAgainst.Except(this.GetGroups().Cast<string>()).ToArray();

        var opponent = groupsToAttack
            .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Where(a => level.WorldToMap(at) == level.WorldToMap(a.Position))
            .FirstOrDefault();

        if (opponent == null)
        {
            return false;
        }

        opponent.GotHit(this, this.AttackPower);
        return true;
    }
}

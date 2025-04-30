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

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Unit);
    }

    public void UnitClicked()
    {
        var level = GetNode<BaseLevel>(LevelPath);

        if (!level.HeaderControl.Character.CanDig)
        {
            return;
        }

        var worldPos = this.RectPosition + this.RectSize / 2;

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
        level.FloatingTextManagerControl.ShowValue((-hitPower).ToString(), this.RectPosition, new Color(1, 0, 0));

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
            newLoot.RectPosition = this.RectPosition;
            this.GetParent().AddChild(newLoot);
        }
    }

    protected Vector2? GetPathToRandomLocation(HashSet<Floor> floors)
    {
        var level = this.GetNode<BaseLevel>(this.LevelPath);
        var pos = level.FloorMap.WorldToMap(this.RectPosition);

        var possibleMoves = level.cardinalDirections
            .Select(dir => pos + dir)
            .Where(cell => floors.Contains((Floor)level.FloorMap.GetCell((int)cell.x, (int)cell.y)))
            // ToDo: GetPathToRandomLocation Should not select blocked locations
            // .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1) 
            .ToList();

        if (possibleMoves.Count <= 0)
        {
            return null;
        }

        return possibleMoves[random.Next(possibleMoves.Count)];
    }

    protected Vector2? GetPathToLoot(HashSet<Floor> floors)
    {
        var level = this.GetNode<BaseLevel>(this.LevelPath);
        var pos = level.FloorMap.WorldToMap(this.RectPosition);

        var loots = this.GetTree()
            .GetNodesInGroup(Groups.Loot)
            .Cast<BaseLoot>()
            .Select(a => level.FloorMap.WorldToMap(a.RectPosition))
            .ToHashSet();

        var pathfinder = new BreadthFirstPathfinder<Vector2>(level);
        pathfinder.Search(pos, loots);
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path.Skip(1).Take(1).FirstOrDefault();
    }

    protected Vector2? GetPathToOtherGroup(HashSet<Floor> floors)
    {
        var groupsToAttack = this.AggroAgainst.Except(this.GetGroups().Cast<string>()).ToArray();

        var level = this.GetNode<BaseLevel>(this.LevelPath);
        var pos = level.FloorMap.WorldToMap(this.RectPosition);
        var otherGroups = groupsToAttack
            .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Select(a => a.RectPosition)
            .Select(a => level.FloorMap.WorldToMap(a))
            .ToHashSet();

        var pathfinder = new BreadthFirstPathfinder<Vector2>(level);
        pathfinder.Search(pos, otherGroups);
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path.Skip(1).Take(1).FirstOrDefault();
    }

    protected bool MoveUnit(Vector2 destination, float speed)
    {
        var direction = destination - this.RectPosition;
        if (direction.LengthSquared() <= speed * speed)
        {
            this.RectPosition = destination;
            return true;
        }

        this.RectPosition += direction.Normalized() * speed;
        return false;
    }

    public virtual void GotHit(BaseUnit from, int attackPower)
    {
        var level = this.GetNode<BaseLevel>(this.LevelPath);

        var hitPower = (uint)Math.Min(attackPower, this.HP);
        this.HP -= hitPower;
        level.FloatingTextManagerControl.ShowValue((-hitPower).ToString(), this.RectPosition, new Color(1, 0, 0));

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

            level.ReFogMap();
        }
    }

    internal bool TryAttackAt(Vector2 at)
    {
        if ((this.RectPosition - at).LengthSquared() >= (this.RectSize / 4).LengthSquared())
        {
            return false;
        }

        var level = this.GetNode<BaseLevel>(this.LevelPath);

        var groupsToAttack = this.AggroAgainst.Except(this.GetGroups().Cast<string>()).ToArray();

        var opponent = groupsToAttack
            .SelectMany(a => this.GetTree().GetNodesInGroup(a).Cast<BaseUnit>())
            .Where(a => level.FloorMap.WorldToMap(at) == level.FloorMap.WorldToMap(a.RectPosition))
            .FirstOrDefault();

        if (opponent == null)
        {
            return false;
        }

        opponent.GotHit(this, this.AttackPower);
        return true;
    }
}

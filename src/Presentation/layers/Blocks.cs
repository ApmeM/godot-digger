using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BrainAI.Pathfinding;
using Godot;

public static class Blocks
{
    public static ValueTuple<int, int, int> Wood = (9, 0, 0);
    public static ValueTuple<int, int, int> Steel = (8, 0, 0);
    public static ValueTuple<int, int, int> Wardrobe = (6, 0, 0);
    public static ValueTuple<int, int, int> Grass = (7, 0, 0);
    public static ValueTuple<int, int, int> Shopkeeper = (1, 0, 0);
    public static ValueTuple<int, int, int> Blacksmith = (2, 0, 0);
    public static ValueTuple<int, int, int> RedHat = (3, 0, 0);
    public static ValueTuple<int, int, int> Tree = (4, 0, 0);
    public static ValueTuple<int, int, int> Wolf = (5, 0, 0);
    public static ValueTuple<int, int, int> Wall = (10, 0, 0);
    public static ValueTuple<int, int, int> Fish = (11, 0, 0);
    public static ValueTuple<int, int, int> Wasp = (12, 0, 0);
    public static ValueTuple<int, int, int> WaspNest = (13, 0, 0);
    public static ValueTuple<int, int, int> StairsUp = (18, 0, 0);
    public static ValueTuple<int, int, int> StairsDown = (17, 0, 0);
    public static ValueTuple<int, int, int> Sign = (16, 0, 0);
    public static ValueTuple<int, int, int> Woodcutter = (20, 0, 0);
    public static ValueTuple<int, int, int> BlacksmithHouse = (14, 0, 0);
    public static ValueTuple<int, int, int> Inn = (15, 0, 0);
    public static ValueTuple<int, int, int> Stash = (19, 0, 0);
    public static ValueTuple<int, int, int> Grandma = (21, 0, 0);
    public static ValueTuple<int, int, int> Door = (22, 0, 0);
    public static ValueTuple<int, int, int> Slime = (23, 0, 0);
    public static ValueTuple<int, int, int> Tree2 = (25, 0, 0);
}

public class BlocksDefinition
{
    private static Random random = new Random();

    public void OnClickMove(BaseLevel level, Vector2 pos)
    {
        if (this.IsDead)
        {
            return;
        }

        var player = new BlocksDefinition
        {
            HP = level.HeaderControl.CurrentHp,
            AttackPower = (int)level.HeaderControl.Character.DigPower,
            ShouldShowDead = true
        };

        const float floatingDelay = 0.3f;
        var currentFloatingsDelay = 0f;
        var actions1 = Battle(level, this, player, pos);
        var popupPos1 = level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2;
        foreach (var action in actions1)
        {
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, action, popupPos1);
            currentFloatingsDelay += floatingDelay;
        }

        currentFloatingsDelay = 0f;
        var actions2 = Battle(level, player, this, pos);
        var popupPos2 = level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2;
        foreach (var action in actions2)
        {
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, action, popupPos2);
            currentFloatingsDelay += floatingDelay;
        }

        level.HeaderControl.CurrentHp = player.HP;
        if (player.IsDead)
        {
            level.HeaderControl.AddBuff(Buff.Dead);
        }
    }

    private static void OnClickSpawn(BaseLevel level, BlocksDefinition definition, Vector2 pos)
    {
        var possibleSpawns = level.cardinalDirections
            .Select(dir => pos + dir)
            .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1)
            .ToList();

        if (possibleSpawns.Count > 0)
        {
            var spawn = possibleSpawns[random.Next(possibleSpawns.Count)];
            level.Meta[spawn] = BlocksDefinition.KnownBlocks[definition.SpawnEnemy.Value].Clone();
            level.Meta[spawn].Group = definition.Group;
            level.Meta[spawn].IsAgressive = definition.IsAgressive;
            level.BlocksMap.SetCellv(spawn, definition.SpawnEnemy.Value.Item1, autotileCoord: new Vector2(definition.SpawnEnemy.Value.Item2, definition.SpawnEnemy.Value.Item3));
        }
    }

    public bool OnTickMove(BaseLevel level, Vector2 pos, float delta)
    {
        if (this.IsDead)
        {
            return false;
        }

        if (!OnTickCanAct(delta))
        {
            return false;
        }

        Vector2? path = null;
        if (this.IsAgressive && path == null && this.AttackPower > 0)
        {
            path = GetPathToOtherGroup(level, pos);
        }
        if (this.MoveToLoot && path == null)
        {
            path = GetPathToLoot(level, pos);
        }
        if (path == null)
        {
            path = GetPathToRandomLocation(level, pos);
        }
        if (path != null)
        {
            OnTickFollowPath(level, pos, path.Value);
            pos = path.Value;
        }
        if (this.CanPickLoot)
        {
            OnTickTryGrabLoot(level, pos);
        }
        if (this.IsAgressive && this.AttackPower > 0)
        {
            OnTickTryAttackOtherGroup(level, pos);
        }
        return path != null;
    }

    private void OnTickTryAttackOtherGroup(BaseLevel level, Vector2 pos)
    {
        var opponent = level.cardinalDirections
            .Select(a => a + pos)
            .Where(a => level.Meta.ContainsKey(a))
            .Select(a => (a, level.Meta[a]))
            .Where(a => a.Item2.Group != this.Group)
            .Where(a => a.Item2.Group != -1)
            .OrderBy(a => a.Item2.HP)
            .FirstOrDefault();

        if (opponent.Item2 == null)
        {
            return;
        }

        const float floatingDelay = 0.3f;
        var currentFloatingsDelay = 0f;
        var actions1 = Battle(level, this, opponent.Item2, opponent.Item1);
        var popupPos1 = level.BlocksMap.MapToWorld(opponent.Item1) + level.BlocksMap.CellSize / 2;
        foreach (var action in actions1)
        {
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, action, popupPos1);
            currentFloatingsDelay += floatingDelay;
        }

        currentFloatingsDelay = 0f;
        var actions2 = Battle(level, opponent.Item2, this, pos);
        var popupPos2 = level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2;
        foreach (var action in actions2)
        {
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, action, popupPos2);
            currentFloatingsDelay += floatingDelay;
        }
    }

    private static List<Control> Battle(BaseLevel level, BlocksDefinition attacker, BlocksDefinition defender, Vector2 defenderPos)
    {
        var result = new List<Control>();

        if (attacker.AttackPower > 0 && defender.HP > 0)
        {
            var hitPower = (uint)Math.Min(attacker.AttackPower, defender.HP);
            defender.HP -= hitPower;
            var label = (Label)level.FloatingTextManagerControl.defaultControl.Duplicate();
            label.Text = $"-{hitPower}";
            label.Modulate = new Color(1, 0, 0);
            result.Add(label);
            if (defender.HP == 0)
            {
                defender.IsDead |= true;
                if (defender.ShouldShowDead)
                {
                    var buffPath = $"res://Presentation/buffs/{Buff.Dead}.tscn";
                    var buffInstance = ResourceLoader.Load<PackedScene>(buffPath).Instance<BaseBuff>();
                    result.Add(buffInstance);
                }
            }
        }

        if (defender.SpawnEnemy.HasValue)
        {
            OnClickSpawn(level, defender, defenderPos);
        }

        if (!defender.IsAgressive)
        {
            defender.IsAgressive = true;
            if (defender.Group >= 0)
            {
                foreach (var kvp in level.Meta)
                {
                    if (defender.Group == kvp.Value.Group)
                    {
                        kvp.Value.IsAgressive = true;
                    }
                }
            }
        }

        return result;
    }

    private bool OnTickCanAct(float delta)
    {
        if (this.MoveDelay == 0)
        {
            return false;
        }

        if (this.CurrentMoveDelay == 0)
        {
            this.CurrentMoveDelay = (float)random.NextDouble() * this.MoveDelay;
        }

        this.CurrentMoveDelay -= delta;

        if (this.CurrentMoveDelay <= 0)
        {
            this.CurrentMoveDelay = this.MoveDelay;
            return true;
        }

        return false;
    }

    private Vector2? GetPathToRandomLocation(BaseLevel level, Vector2 pos)
    {
        var possibleMoves = level.cardinalDirections
            .Select(dir => pos + dir)
            .Where(cell => this.MoveFloors.Contains((level.FloorMap.GetCell((int)cell.x, (int)cell.y), 0, 0)))
            .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1)
            .ToList();

        if (possibleMoves.Count <= 0)
        {
            return null;
        }

        return possibleMoves[random.Next(possibleMoves.Count)];
    }

    private Vector2? GetPathToLoot(BaseLevel level, Vector2 pos)
    {
        var pathfinder = new BreadthFirstPathfinder<Vector2>(level);
        pathfinder.Search(pos, level.LootMap.GetUsedCells().Cast<Vector2>().ToHashSet());
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path.Skip(1).Take(1).FirstOrDefault();
    }

    private Vector2? GetPathToOtherGroup(BaseLevel level, Vector2 pos)
    {
        var pathfinder = new BreadthFirstPathfinder<Vector2>(level);
        pathfinder.Search(pos, level.Meta
            .Where(a => a.Value.Group != this.Group)
            .Where(a => a.Value.Group >= 0)
            .Select(a => a.Key)
            .SelectMany(a => level.cardinalDirections.Select(b => b + a))
            .ToHashSet());
        var path = pathfinder.ResultPath;

        if (path == null || path.Count < 2)
        {
            return null;
        }

        return path.Skip(1).Take(1).FirstOrDefault();
    }

    private void OnTickFollowPath(BaseLevel level, Vector2 pos, Vector2 move)
    {
        level.Meta[move] = level.Meta[pos];

        level.BlocksMap.SetCellv(move, level.BlocksMap.GetCellv(pos), autotileCoord: level.BlocksMap.GetCellAutotileCoord((int)pos.x, (int)pos.y));
        level.BlocksMap.SetCellv(pos, -1);

        level.Meta.Remove(pos);
    }

    private void OnTickTryGrabLoot(BaseLevel level, Vector2 pos)
    {
        var loot = level.LootMap.GetCellv(pos);
        if (loot == -1)
        {
            return;
        }

        this.Loot.Add((loot, 0, 0));
        level.LootMap.SetCellv(pos, -1);
    }

    public static Dictionary<ValueTuple<int, int, int>, BlocksDefinition> KnownBlocks = new Dictionary<ValueTuple<int, int, int>, BlocksDefinition>{
        { Blocks.Wood,            new BlocksDefinition{HP = 2,} },
        { Blocks.Steel,           new BlocksDefinition{HP = 3,} },
        { Blocks.Wardrobe,        new BlocksDefinition{HP = 4,} },
        { Blocks.Grass,           new BlocksDefinition{HP = 1,} },
        { Blocks.Shopkeeper,      new BlocksDefinition{} },
        { Blocks.Blacksmith,      new BlocksDefinition{} },
        { Blocks.RedHat,          new BlocksDefinition{} },
        { Blocks.Tree,            new BlocksDefinition{HP = 3,} },
        { Blocks.Tree2,           new BlocksDefinition{HP = 3,} },
        { Blocks.Wolf,            new BlocksDefinition{HP = 2, ShouldShowDead = true, IsAgressive = true, AttackPower = 4,  MoveDelay = 5,    MoveFloors = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles} }},
        { Blocks.Wall,            new BlocksDefinition{} },
        { Blocks.Fish,            new BlocksDefinition{                          MoveDelay = 1,    MoveFloors = new HashSet<(int, int, int)>{Floor.Water} }},
        { Blocks.Wasp,            new BlocksDefinition{HP = 2, ShouldShowDead = true, AttackPower = 1, MoveDelay = 0.5f, MoveFloors = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles, Floor.Water} }},
        { Blocks.WaspNest,        new BlocksDefinition{HP = 3, ShouldShowDead = true, SpawnEnemy = Blocks.Wasp} },
        { Blocks.StairsUp,        new BlocksDefinition{} },
        { Blocks.StairsDown,      new BlocksDefinition{} },
        { Blocks.Sign,            new BlocksDefinition{        NoFogBlocker = true} },
        { Blocks.Woodcutter,      new BlocksDefinition{} },
        { Blocks.BlacksmithHouse, new BlocksDefinition{} },
        { Blocks.Inn,             new BlocksDefinition{} },
        { Blocks.Stash,           new BlocksDefinition{} },
        { Blocks.Grandma,         new BlocksDefinition{} },
        { Blocks.Door,            new BlocksDefinition{} },
        { Blocks.Slime,           new BlocksDefinition{HP = 2, ShouldShowDead = true, AttackPower = 1,  MoveDelay = 2, MoveFloors = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles}, CanPickLoot = true, MoveToLoot = true }},
    };

    public BlocksDefinition Clone()
    {
        return new BlocksDefinition
        {
            HP = this.HP,
            NoFogBlocker = this.NoFogBlocker,
            MoveDelay = this.MoveDelay,
            CanPickLoot = this.CanPickLoot,
            MoveToLoot = this.MoveToLoot,
            MoveFloors = new HashSet<(int, int, int)>(this.MoveFloors),
            AttackPower = this.AttackPower,
            SpawnEnemy = this.SpawnEnemy,
            IsDead = this.IsDead,
            Loot = new List<(int, int, int)>(this.Loot),
            Group = this.Group,
            ShouldShowDead = this.ShouldShowDead
        };
    }

    public uint HP;
    public bool NoFogBlocker;
    public float MoveDelay;
    public float CurrentMoveDelay;
    public bool CanPickLoot;
    public bool MoveToLoot;
    public HashSet<(int, int, int)> MoveFloors = new HashSet<(int, int, int)>();
    public int AttackPower;
    public (int, int, int)? SpawnEnemy;
    public bool IsDead;
    public List<(int, int, int)> Loot = new List<(int, int, int)>();
    public int Group;
    public bool IsAgressive = false;
    public bool ShouldShowDead = false;
}

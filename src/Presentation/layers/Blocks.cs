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

    public bool OnClickMove(BaseLevel level, Vector2 pos)
    {
        var isDead = false;
        if (this.HP > 0)
        {
            isDead = OnClickDefend(level, pos);
        }
        if (this.AttackPower > 0)
        {
            OnClickAttack(level, pos);
        }
        if (this.SpawnEnemy.HasValue)
        {
            OnClickSpawn(level, pos);
        }
        return isDead;
    }

    private bool OnClickDefend(BaseLevel level, Vector2 pos)
    {
        const float floatingDelay = 0.3f;
        var currentFloatingsDelay = 0f;

        var hitPower = Math.Min(this.HP, level.HeaderControl.Character.DigPower);
        this.HP -= hitPower;
        level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, (-hitPower).ToString(), level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2, new Color(1, 1, 0));
        currentFloatingsDelay += floatingDelay;
        return this.HP == 0;
    }

    private void OnClickAttack(BaseLevel level, Vector2 pos)
    {
        const float floatingDelay = 0.3f;
        var currentFloatingsDelay = 0f;

        if (this.AttackPower > level.HeaderControl.CurrentHp)
        {
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, (-level.HeaderControl.CurrentHp).ToString(), level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2, new Color(1, 0, 0));
            currentFloatingsDelay += floatingDelay;
            level.HeaderControl.CurrentHp = 0;
            var buff = level.HeaderControl.AddBuff(Buff.Dead);
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, (Control)buff.Duplicate(), level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2);
            currentFloatingsDelay += floatingDelay;
        }
        else
        {
            level.HeaderControl.CurrentHp -= (uint)this.AttackPower;
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, (-this.AttackPower).ToString(), level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2, new Color(1, 0, 0));
            currentFloatingsDelay += floatingDelay;
        }
    }

    private void OnClickSpawn(BaseLevel level, Vector2 pos)
    {
        var possibleSpawns = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right }
            .Select(dir => pos + dir)
            .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1)
            .ToList();

        if (possibleSpawns.Count > 0)
        {
            var spawn = possibleSpawns[random.Next(possibleSpawns.Count)];
            level.Meta[spawn] = BlocksDefinition.KnownBlocks[this.SpawnEnemy.Value].Clone();
            level.BlocksMap.SetCellv(spawn, this.SpawnEnemy.Value.Item1, autotileCoord: new Vector2(this.SpawnEnemy.Value.Item2, this.SpawnEnemy.Value.Item3));
        }
    }

    public bool OnTickMove(BaseLevel level, Vector2 pos, float delta)
    {
        var result = OnTickCanAct(delta) &&
                (this.MoveToLoot && OnTickMoveToLootInternal(level, pos) || OnTickRandomMoveInternal(level, pos)) &&
                OnTickFollowPath(level, pos);
        if (this.CanPickLoot)
        {
            OnTickTryGrabLoot(level, pos);
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

    private bool OnTickRandomMoveInternal(BaseLevel level, Vector2 pos)
    {
        var possibleMoves = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right }
            .Select(dir => pos + dir)
            .Where(cell => this.MoveFloors.Contains((level.FloorMap.GetCell((int)cell.x, (int)cell.y), 0, 0)))
            .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1)
            .ToList();

        if (possibleMoves.Count <= 0)
        {
            return false;
        }

        this.Path = possibleMoves[random.Next(possibleMoves.Count)];
        return true;
    }

    private bool OnTickFollowPath(BaseLevel level, Vector2 pos)
    {
        var move = this.Path;

        level.Meta[move] = level.Meta[pos];

        level.BlocksMap.SetCellv(move, level.BlocksMap.GetCellv(pos), autotileCoord: level.BlocksMap.GetCellAutotileCoord((int)pos.x, (int)pos.y));
        level.BlocksMap.SetCellv(pos, -1);

        level.Meta.Remove(pos);

        return true;
    }

    private bool OnTickMoveToLootInternal(BaseLevel level, Vector2 pos)
    {
        var pathfinder = new BreadthFirstPathfinder<Vector2>(level);
        pathfinder.Search(pos, level.LootMap.GetUsedCells().Cast<Vector2>().ToHashSet());
        var path = pathfinder.ResultPath;

        if (path == null || path.Count == 0)
        {
            return false;
        }

        this.Path = path.Skip(1).Take(1).FirstOrDefault();
        return true;
    }

    private void OnTickTryGrabLoot(BaseLevel level, Vector2 pos)
    {
        var loot = level.LootMap.GetCellv(pos);
        if (loot == -1)
        {
            return;
        }

        this.Loot.Add((loot, 0, 0));
        GD.Print(this.Loot.Count);
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
        { Blocks.Wolf,            new BlocksDefinition{HP = 2, AttackPower = 4,  MoveDelay = 5,    MoveFloors = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles} }},
        { Blocks.Wall,            new BlocksDefinition{} },
        { Blocks.Fish,            new BlocksDefinition{                          MoveDelay = 1,    MoveFloors = new HashSet<(int, int, int)>{Floor.Water} }},
        { Blocks.Wasp,            new BlocksDefinition{HP = 2, AttackPower = 10, MoveDelay = 0.5f, MoveFloors = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles, Floor.Water} }},
        { Blocks.WaspNest,        new BlocksDefinition{HP = 3, SpawnEnemy = Blocks.Wasp} },
        { Blocks.StairsUp,        new BlocksDefinition{} },
        { Blocks.StairsDown,      new BlocksDefinition{} },
        { Blocks.Sign,            new BlocksDefinition{        NoFogBlocker = true} },
        { Blocks.Woodcutter,      new BlocksDefinition{} },
        { Blocks.BlacksmithHouse, new BlocksDefinition{} },
        { Blocks.Inn,             new BlocksDefinition{} },
        { Blocks.Stash,           new BlocksDefinition{} },
        { Blocks.Grandma,         new BlocksDefinition{} },
        { Blocks.Door,            new BlocksDefinition{} },
        { Blocks.Slime,           new BlocksDefinition{HP = 2, AttackPower = 1,  MoveDelay = 2, MoveFloors = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles}, CanPickLoot = true, MoveToLoot = true }},
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
            Loot = new List<(int, int, int)>(this.Loot)
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
    public Vector2 Path;
}

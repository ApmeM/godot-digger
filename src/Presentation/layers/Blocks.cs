using System;
using System.Collections.Generic;
using System.Linq;
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

}

public class BlocksDefinition
{
    private static Random random = new Random();

    private static void OnClickAttack(BaseLevel level, Vector2 pos, int power)
    {
        const float floatingDelay = 0.3f;
        var currentFloatingsDelay = 0f;

        if (power > level.HeaderControl.CurrentHp)
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
            level.HeaderControl.CurrentHp -= (uint)power;
            level.FloatingTextManagerControl.ShowValueDelayed(currentFloatingsDelay, (-power).ToString(), level.BlocksMap.MapToWorld(pos) + level.BlocksMap.CellSize / 2, new Color(1, 0, 0));
            currentFloatingsDelay += floatingDelay;
        }
    }

    private static void OnCickSpawn(BaseLevel level, Vector2 pos, ValueTuple<int, int, int> spawnBlock)
    {
        var possibleSpawns = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right }
            .Select(dir => pos + dir)
            .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1)
            .ToList();

        if (possibleSpawns.Count > 0)
        {
            var spawn = possibleSpawns[random.Next(possibleSpawns.Count)];
            level.BlocksMap.SetCellv(spawn, spawnBlock.Item1, autotileCoord: new Vector2(spawnBlock.Item2, spawnBlock.Item3));
        }
    }

    private static bool OnTickRandomMove(BaseLevel level, Vector2 pos, float delta, float moveDelay, params (int, int, int)[] moveFloors)
    {
        var moveFloor = new HashSet<(int, int, int)>(moveFloors);
        var metaName = $"Move_{pos}";
        if (!level.BlocksMap.HasMeta(metaName))
        {
            level.BlocksMap.SetMeta(metaName, random.NextDouble() * moveDelay);
        }

        if ((float)level.BlocksMap.GetMeta(metaName) >= 0)
        {
            level.BlocksMap.SetMeta(metaName, (float)level.BlocksMap.GetMeta(metaName) - delta);
            return false;
        }

        level.BlocksMap.SetMeta(metaName, moveDelay);

        var possibleMoves = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Up, Vector2.Right }
            .Select(dir => pos + dir)
            .Where(cell => moveFloor.Contains((level.FloorMap.GetCell((int)cell.x, (int)cell.y), 0, 0)))
            .Where(cell => level.BlocksMap.GetCell((int)cell.x, (int)cell.y) == -1)
            .ToList();

        if (possibleMoves.Count <= 0)
        {
            return false;
        }

        var move = possibleMoves[random.Next(possibleMoves.Count)];
        level.BlocksMap.SetCellv(move, level.BlocksMap.GetCellv(pos), autotileCoord: level.BlocksMap.GetCellAutotileCoord((int)pos.x, (int)pos.y));
        level.BlocksMap.SetCellv(pos, -1);

        var cellString = pos.ToString();
        foreach (var meta in level.BlocksMap.GetMetaList().Where(a => a.EndsWith(cellString)))
        {
            var metaString = meta.Substring(0, meta.Length - cellString.Length);
            level.BlocksMap.SetMeta(metaString + move, level.BlocksMap.GetMeta(meta));
            level.BlocksMap.SetMeta(meta, null);
        }
        return true;
    }

    public static Dictionary<ValueTuple<int, int, int>, BlocksDefinition> KnownBlocks = new Dictionary<ValueTuple<int, int, int>, BlocksDefinition>{
        { Blocks.Wood,            new BlocksDefinition{HP = 2                    } },
        { Blocks.Steel,           new BlocksDefinition{HP = 3                    } },
        { Blocks.Wardrobe,        new BlocksDefinition{HP = 4                    } },
        { Blocks.Grass,           new BlocksDefinition{HP = 1                    } },
        { Blocks.Shopkeeper,      new BlocksDefinition{                          } },
        { Blocks.Blacksmith,      new BlocksDefinition{                          } },
        { Blocks.RedHat,          new BlocksDefinition{                          } },
        { Blocks.Tree,            new BlocksDefinition{HP = 3                    } },
        { Blocks.Wolf,            new BlocksDefinition{HP = 2,                     OnClickAction = (level, pos) => {OnClickAttack(level, pos, 4);},         OnTickAction = (level, pos, delta) => {return OnTickRandomMove(level, pos, delta, 5, Floor.Ground, Floor.Tiles);} }},
        { Blocks.Wall,            new BlocksDefinition{                          } },
        { Blocks.Fish,            new BlocksDefinition{                                                                                                     OnTickAction = (level, pos, delta) => {return OnTickRandomMove(level, pos, delta, 1, Floor.Water);} }},
        { Blocks.Wasp,            new BlocksDefinition{HP = 2,                     OnClickAction = (level, pos) => {OnClickAttack(level, pos, 10);},        OnTickAction = (level, pos, delta) => {return OnTickRandomMove(level, pos, delta, 0.5f, Floor.Ground, Floor.Tiles, Floor.Water);} }},
        { Blocks.WaspNest,        new BlocksDefinition{HP = 3,                     OnClickAction = (level, pos) => {OnCickSpawn(level, pos, Blocks.Wasp);}} },
        { Blocks.StairsUp,        new BlocksDefinition{                          } },
        { Blocks.StairsDown,      new BlocksDefinition{                          } },
        { Blocks.Sign,            new BlocksDefinition{        FogBlocker = false} },
        { Blocks.Woodcutter,      new BlocksDefinition{                          } },
        { Blocks.BlacksmithHouse, new BlocksDefinition{                          } },
        { Blocks.Inn,             new BlocksDefinition{                          } },
        { Blocks.Stash,           new BlocksDefinition{                          } },
        { Blocks.Grandma,         new BlocksDefinition{                          } },
        { Blocks.Door,            new BlocksDefinition{                          } },
        { Blocks.Slime,           new BlocksDefinition{HP = 2,                     OnClickAction = (level, pos) => {OnClickAttack(level, pos, 1);},         OnTickAction = (level, pos, delta) => {return OnTickRandomMove(level, pos, delta, 2, Floor.Ground, Floor.Tiles);} }},
    };

    public uint HP;

    public bool FogBlocker = true;

    public Action<BaseLevel, Vector2> OnClickAction;
    public Func<BaseLevel, Vector2, float, bool> OnTickAction;
}

using System;
using System.Collections.Generic;
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

public class BlocksDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> CustomClicked = (level, pos) => { level.CustomBlockClicked(pos); };
    public static Dictionary<ValueTuple<int, int, int>, BlocksDefinition> KnownBlocks = new Dictionary<ValueTuple<int, int, int>, BlocksDefinition>{
        { Blocks.Wood, new BlocksDefinition{HP = 2 } },
        { Blocks.Steel, new BlocksDefinition{HP = 3} },
        { Blocks.Wardrobe, new BlocksDefinition{HP = 4} },
        { Blocks.Grass, new BlocksDefinition{HP = 1} },
        { Blocks.Shopkeeper, new BlocksDefinition{HP = 0} },
        { Blocks.Blacksmith, new BlocksDefinition{HP = 0} },
        { Blocks.RedHat, new BlocksDefinition{HP = 0} },
        { Blocks.Tree, new BlocksDefinition{HP = 3} },
        { Blocks.Wolf, new BlocksDefinition{HP = 2, Attack = 4, MoveDelay=5, MoveFloor = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles}} },
        { Blocks.Wall, new BlocksDefinition{HP = 0} },
        { Blocks.Fish, new BlocksDefinition{HP = 0, MoveDelay = 1, MoveFloor = new HashSet<(int, int, int)>{Floor.Water}} },
        { Blocks.Wasp, new BlocksDefinition{HP = 2, Attack = 10, MoveDelay = 0.5f, MoveFloor = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles, Floor.Water}} },
        { Blocks.WaspNest, new BlocksDefinition{HP = 3, SpawnBlock = Blocks.Wasp} },
        { Blocks.StairsUp, new BlocksDefinition{HP = 0} },
        { Blocks.StairsDown, new BlocksDefinition{HP = 0} },
        { Blocks.Sign, new BlocksDefinition{HP = 0, FogBlocker = false} },
        { Blocks.Woodcutter, new BlocksDefinition{HP = 0} },
        { Blocks.BlacksmithHouse, new BlocksDefinition{HP = 0} },
        { Blocks.Inn, new BlocksDefinition{HP = 0} },
        { Blocks.Stash, new BlocksDefinition{HP = 0} },
        { Blocks.Grandma, new BlocksDefinition{HP = 0} },
        { Blocks.Door, new BlocksDefinition{HP = 0} },
        { Blocks.Slime, new BlocksDefinition{HP = 2, Attack = 1, MoveDelay = 2, MoveFloor = new HashSet<(int, int, int)>{Floor.Ground, Floor.Tiles}} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; } = CustomClicked;

    public int Attack;

    public uint HP;

    // Fog Blocker
    public bool FogBlocker = true;

    // Move data
    public float MoveDelay;
    public HashSet<ValueTuple<int, int, int>> MoveFloor = new HashSet<(int, int, int)>();

    // Spawn data
    public ValueTuple<int, int, int> SpawnBlock = (-1, -1, -1);
}

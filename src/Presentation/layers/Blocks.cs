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
}

public class BlocksDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };
    private static Action<BaseLevel, Vector2> DigBlock = (level, pos) => { level.TryDigBlock(pos); };
    private static Action<BaseLevel, Vector2> CustomClicked = (level, pos) => { level.CustomBlockClicked(pos); };
    public static Dictionary<ValueTuple<int, int, int>, BlocksDefinition> KnownBlocks = new Dictionary<ValueTuple<int, int, int>, BlocksDefinition>{
        { Blocks.Wood, new BlocksDefinition{HP = 2, ClickAction=DigBlock } },
        { Blocks.Steel, new BlocksDefinition{HP = 3, ClickAction=DigBlock} },
        { Blocks.Wardrobe, new BlocksDefinition{HP = 4, ClickAction=DigBlock} },
        { Blocks.Grass, new BlocksDefinition{HP = 1, ClickAction=DigBlock} },
        { Blocks.Shopkeeper, new BlocksDefinition{HP = 0, ClickAction=CustomClicked} },
        { Blocks.Blacksmith, new BlocksDefinition{HP = 0, ClickAction=CustomClicked} },
        { Blocks.RedHat, new BlocksDefinition{HP = 0, ClickAction=CustomClicked} },
        { Blocks.Tree, new BlocksDefinition{HP = 3, ClickAction=DigBlock} },
        { Blocks.Wolf, new BlocksDefinition{HP = 2, ClickAction=DigBlock} },
        { Blocks.Wall, new BlocksDefinition{ClickAction = DoNothing} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }

    public uint HP;
}

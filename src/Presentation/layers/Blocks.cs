using System;
using System.Collections.Generic;
using Godot;

public static class Blocks
{
    public static ValueTuple<int, int, int> Wood = (0, 0, 0);
    public static ValueTuple<int, int, int> Steel = (0, 1, 0);
    public static ValueTuple<int, int, int> Wardrobe = (0, 2, 0);
    public static ValueTuple<int, int, int> Grass = (0, 3, 0);
    public static ValueTuple<int, int, int> Shopkeeper = (1, 0, 0);
}

public class BlocksDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DigBlock = (level, pos) => { level.TryDigBlock(pos); };
    private static Action<BaseLevel, Vector2> ShowPopup = (level, pos) => { level.ShowPopup(pos); };
    public static Dictionary<ValueTuple<int, int, int>, BlocksDefinition> KnownBlocks = new Dictionary<ValueTuple<int, int, int>, BlocksDefinition>{
        { Blocks.Wood, new BlocksDefinition{HP = 2, ClickAction=DigBlock } },
        { Blocks.Steel, new BlocksDefinition{HP = 3, ClickAction=DigBlock} },
        { Blocks.Wardrobe, new BlocksDefinition{HP = 4, ClickAction=DigBlock} },
        { Blocks.Grass, new BlocksDefinition{HP = 1, ClickAction=DigBlock} },
        { Blocks.Shopkeeper, new BlocksDefinition{HP = 0, ClickAction=ShowPopup} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }

    public uint HP;
}

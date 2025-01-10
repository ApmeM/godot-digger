using System;
using System.Collections.Generic;
using Godot;

public static class Blocks
{
    public static int Wood = 0;
    public static int Steel = 1;
    public static int Wardrobe = 2;
    public static int Grass = 3;
}

public class BlocksDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DigBlock = (level, pos) => { level.TryDigBlock(pos); };
    public static Dictionary<int, BlocksDefinition> KnownBlocks = new Dictionary<int, BlocksDefinition>{
        { Blocks.Wood, new BlocksDefinition{HP = 2, ClickAction=DigBlock } },
        { Blocks.Steel, new BlocksDefinition{HP = 3, ClickAction=DigBlock} },
        { Blocks.Wardrobe, new BlocksDefinition{HP = 4, ClickAction=DigBlock} },
        { Blocks.Grass, new BlocksDefinition{HP = 1, ClickAction=DigBlock} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }

    public uint HP;
}

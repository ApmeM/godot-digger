using System;
using System.Collections.Generic;
using Godot;

public static class Floor
{
    public static int Tiles = 0;
    public static int Wall = 1;
    public static int Ground = 2;
}

public class FloorDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };

    public static Dictionary<int, FloorDefinition> KnownFloors = new Dictionary<int, FloorDefinition>{
        { Floor.Wall, new FloorDefinition{ClickAction = DoNothing} },
        { Floor.Tiles, new FloorDefinition{ClickAction = DoNothing} },
        { Floor.Ground, new FloorDefinition{ClickAction = DoNothing} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

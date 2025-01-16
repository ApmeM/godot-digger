using System;
using System.Collections.Generic;
using Godot;

public static class Floor
{
    public static ValueTuple<int, int, int> Tiles = (3, 0, 0);
    public static ValueTuple<int, int, int> Ground = (2, 0, 0);
    public static ValueTuple<int, int, int> Water = (1, 0, 0);
}

public class FloorDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };

    public static Dictionary<ValueTuple<int, int, int>, FloorDefinition> KnownFloors = new Dictionary<ValueTuple<int, int, int>, FloorDefinition>{
        { Floor.Tiles, new FloorDefinition{ClickAction = DoNothing} },
        { Floor.Ground, new FloorDefinition{ClickAction = DoNothing} },
        { Floor.Water, new FloorDefinition{ClickAction = DoNothing} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

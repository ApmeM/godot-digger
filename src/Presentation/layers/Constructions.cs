using System;
using System.Collections.Generic;
using Godot;

public static class Constructions
{
    public static ValueTuple<int, int, int> Grass = (6,0,0);
    public static ValueTuple<int, int, int> StatueLeft = (12,0,0);
    public static ValueTuple<int, int, int> StatueRight = (13,0,0);
    public static ValueTuple<int, int, int> OpenDoor = (5,0,0);
    public static ValueTuple<int, int, int> Stump = (2,0,0);
}

public class ConstructionsDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };

    public static Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition> KnownConstructions = new Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition>{
        { Constructions.Grass, new ConstructionsDefinition{} },
        { Constructions.StatueLeft, new ConstructionsDefinition{} },
        { Constructions.StatueRight, new ConstructionsDefinition{} },
        { Constructions.OpenDoor, new ConstructionsDefinition{} },
        { Constructions.Stump, new ConstructionsDefinition{} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; } = DoNothing;
}

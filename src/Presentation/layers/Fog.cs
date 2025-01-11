using System;
using System.Collections.Generic;
using Godot;

public static class Fog
{
    public static ValueTuple<int, int, int> Basic = (0, 0, 0);
}


public class FogDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };

    public static Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition> KnownConstructions = new Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition>{
        { Fog.Basic, new ConstructionsDefinition{ClickAction = DoNothing} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

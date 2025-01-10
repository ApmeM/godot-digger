using System;
using System.Collections.Generic;
using Godot;

public static class Fog
{
    public static int Basic = 0;
}


public class FogDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };

    public static Dictionary<int, ConstructionsDefinition> KnownConstructions = new Dictionary<int, ConstructionsDefinition>{
        { Fog.Basic, new ConstructionsDefinition{ClickAction = DoNothing} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

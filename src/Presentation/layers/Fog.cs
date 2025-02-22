using System;
using System.Collections.Generic;
using Godot;

public static class Fog
{
    public static ValueTuple<int, int, int> Basic = (0, 0, 0);
    public static ValueTuple<int, int, int> UnfogStart = (1, 0, 0);
    public static ValueTuple<int, int, int> NoFog = (2, 0, 0);
}


public class FogDefinition
{
    public static Dictionary<ValueTuple<int, int, int>, FogDefinition> KnownFog = new Dictionary<ValueTuple<int, int, int>, FogDefinition>{
        { Fog.Basic, new FogDefinition{} },
        { Fog.UnfogStart, new FogDefinition{} },
        { Fog.NoFog, new FogDefinition{} },
    };
}

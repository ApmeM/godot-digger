using System;
using System.Collections.Generic;

public static class Floor
{
    public static ValueTuple<int, int, int> Tiles = (3, 0, 0);
    public static ValueTuple<int, int, int> Ground = (2, 0, 0);
    public static ValueTuple<int, int, int> Water = (1, 0, 0);
    public static ValueTuple<int, int, int> Grass = (4, 0, 0);
    public static ValueTuple<int, int, int> Stump = (5, 0, 0);
}

public class FloorDefinition
{
    public static Dictionary<ValueTuple<int, int, int>, FloorDefinition> KnownFloors = new Dictionary<ValueTuple<int, int, int>, FloorDefinition>{
        { Floor.Tiles, new FloorDefinition{} },
        { Floor.Ground, new FloorDefinition{} },
        { Floor.Water, new FloorDefinition{} },
        { Floor.Grass, new FloorDefinition{} },
        { Floor.Stump, new FloorDefinition{} },
    };
}

using System;
using System.Collections.Generic;
using Godot;

public static class Constructions
{
    public static ValueTuple<int, int, int> StairsUp = (10,0,0);
    public static ValueTuple<int, int, int> StairsDown = (9,0,0);
    public static ValueTuple<int, int, int> Sign = (8,0,0);
    public static ValueTuple<int, int, int> Grass = (6,0,0);
    public static ValueTuple<int, int, int> StatueLeft = (12,0,0);
    public static ValueTuple<int, int, int> StatueRight = (13,0,0);
    public static ValueTuple<int, int, int> OpenDoor = (5,0,0);
    public static ValueTuple<int, int, int> Woodcutter = (14,0,0);
    public static ValueTuple<int, int, int> Blacksmith = (4,0,0);
    public static ValueTuple<int, int, int> Inn = (7,0,0);
    public static ValueTuple<int, int, int> Stash = (11,0,0);
    public static ValueTuple<int, int, int> Fish = (1,0,0);
    public static ValueTuple<int, int, int> Stump = (2,0,0);
    public static ValueTuple<int, int, int> Grandma = (3,0,0);
}

public class ConstructionsDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> CustomConstruction = (level, pos) => { level.CustomConstructionClickedAsync(pos); };

    public static Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition> KnownConstructions = new Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition>{
        { Constructions.StairsUp, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.StairsDown, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Sign, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Grass, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.StatueLeft, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.StatueRight, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.OpenDoor, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Woodcutter, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Blacksmith, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Inn, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Stash, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Fish, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Stump, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Grandma, new ConstructionsDefinition{ClickAction = CustomConstruction} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

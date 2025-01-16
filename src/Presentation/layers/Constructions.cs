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
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };
    private static Action<BaseLevel, Vector2> ChangeLevel = (level, pos) => { level.ChangeLevelClicked(pos); };
    private static Action<BaseLevel, Vector2> ShowPopup = (level, pos) => { level.ShowPopup(pos); };
    private static Action<BaseLevel, Vector2> CustomConstruction = (level, pos) => { level.CustomConstructionClicked(pos); };

    public static Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition> KnownConstructions = new Dictionary<ValueTuple<int, int, int>, ConstructionsDefinition>{
        { Constructions.StairsUp, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.StairsDown, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Sign, new ConstructionsDefinition{ClickAction = ShowPopup} },
        { Constructions.Grass, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.StatueLeft, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.StatueRight, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.OpenDoor, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.Woodcutter, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Blacksmith, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Inn, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Stash, new ConstructionsDefinition{ClickAction = CustomConstruction} },
        { Constructions.Fish, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.Stump, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.Grandma, new ConstructionsDefinition{ClickAction = CustomConstruction} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

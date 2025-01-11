using System;
using System.Collections.Generic;
using Godot;

public static class Constructions
{
    public static ValueTuple<int, int, int> StairsUp = (0,0,0);
    public static ValueTuple<int, int, int> StairsDown = (0,1,0);
    public static ValueTuple<int, int, int> Sign = (0,2,0);
    public static ValueTuple<int, int, int> Grass = (0,3,0);
    public static ValueTuple<int, int, int> StatueLeft = (0,4,0);
    public static ValueTuple<int, int, int> StatueRight = (0,5,0);
    public static ValueTuple<int, int, int> OpenDoor = (0,6,0);
    public static ValueTuple<int, int, int> Woodcutter = (0,7,0);
    public static ValueTuple<int, int, int> Blacksmith = (0,8,0);
    public static ValueTuple<int, int, int> Inn = (0,9,0);
    public static ValueTuple<int, int, int> Stash = (0,10,0);
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
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

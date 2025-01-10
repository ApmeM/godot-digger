using System;
using System.Collections.Generic;
using Godot;

public static class Constructions
{
    public static int StairsUp = 0;
    public static int StairsDown = 1;
    public static int Sign = 2;
    public static int Grass = 3;
    public static int StatueLeft = 4;
    public static int StatueRight = 5;
    public static int OpenDoor = 6;
    public static int Woodcutter = 7;
    public static int Blacksmith = 8;
    public static int Inn = 9;
    public static int Stash = 10;
}

public class ConstructionsDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };
    private static Action<BaseLevel, Vector2> ChangeLevel = (level, pos) => { level.ChangeLevelClicked(pos); };
    private static Action<BaseLevel, Vector2> ShowPopup = (level, pos) => { level.ShowPopup(pos); };
    private static Action<BaseLevel, Vector2> CustomConstruction = (level, pos) => { level.CustomConstructionClicked(pos); };

    public static Dictionary<int, ConstructionsDefinition> KnownConstructions = new Dictionary<int, ConstructionsDefinition>{
        { Constructions.StairsUp, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.StairsDown, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Sign, new ConstructionsDefinition{ClickAction = ShowPopup} },
        { Constructions.Grass, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.StatueLeft, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.StatueRight, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.OpenDoor, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.Woodcutter, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Blacksmith, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Inn, new ConstructionsDefinition{ClickAction = ChangeLevel} },
        { Constructions.Stash, new ConstructionsDefinition{ClickAction = CustomConstruction} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
}

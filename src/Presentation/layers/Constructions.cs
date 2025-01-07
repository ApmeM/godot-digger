using System;
using System.Collections.Generic;
using Godot;

public enum Constructions
{
    StairsUp,
    StairsDown,
    Sign,
    Grass,
    StatueLeft,
    StatueRight,
    OpenDoor,
    Woodcutter,
    Blacksmith,
    Inn,
}

public class ConstructionsDefinition
{
    public static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };

    public static Dictionary<Constructions, ConstructionsDefinition> KnownConstructions = new Dictionary<Constructions, ConstructionsDefinition>{
        { Constructions.StairsUp, new ConstructionsDefinition{ClickAction = (level, pos)=>{level.ChangeLevelClicked(pos);}} },
        { Constructions.StairsDown, new ConstructionsDefinition{ClickAction = (level, pos)=>{level.ChangeLevelClicked(pos);}} },
        { Constructions.Sign, new ConstructionsDefinition{ClickAction = (level, pos)=>{level.ShowPopup(pos);}} },
        { Constructions.Grass, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.StatueLeft, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.StatueRight, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.OpenDoor, new ConstructionsDefinition{ClickAction = DoNothing} },
        { Constructions.Woodcutter, new ConstructionsDefinition{ClickAction = (level, pos)=>{level.ChangeLevelClicked(pos);}} },
        { Constructions.Blacksmith, new ConstructionsDefinition{ClickAction = (level, pos)=>{level.ChangeLevelClicked(pos);}} },
        { Constructions.Inn, new ConstructionsDefinition{ClickAction = (level, pos)=>{level.ChangeLevelClicked(pos);}} },
    };


    public Action<BaseLevel, Vector2> ClickAction;
}

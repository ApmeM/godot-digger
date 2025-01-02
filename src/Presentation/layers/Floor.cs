using System;
using System.Collections.Generic;
using Godot;

public enum Floor
{
    Path,
    Wall,
    StairsUp,
    StairsDown,
    Sign,
}

public class FloorDefinition
{
    public static Dictionary<Floor, FloorDefinition> KnownFloors = new Dictionary<Floor, FloorDefinition>{
        { Floor.Path, new FloorDefinition{ClickAction = (level, pos)=>{}} },
        { Floor.Wall, new FloorDefinition{ClickAction = (level, pos)=>{}} },
        { Floor.StairsUp, new FloorDefinition{ClickAction = (level, pos)=>{level.ExitDungeonClicked();}} },
        { Floor.StairsDown, new FloorDefinition{ClickAction = (level, pos)=>{level.ChangeLevelClicked(pos);}} },
        { Floor.Sign, new FloorDefinition{ClickAction = (level, pos)=>{level.ShowPopup(pos);}} },
    };


    public Action<BaseLevel, Vector2> ClickAction;
}

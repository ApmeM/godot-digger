using System;
using Godot;

public interface IActionDefinition
{
    Action<BaseLevel, Vector2> ClickAction { get; }
}
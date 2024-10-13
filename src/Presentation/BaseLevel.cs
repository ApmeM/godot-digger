using Godot;
using GodotAnalysers;
using System;

[SceneReference("BaseLevel.tscn")]
public partial class BaseLevel
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

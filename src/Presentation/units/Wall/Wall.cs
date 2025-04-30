using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Wall.tscn")]
public partial class Wall
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

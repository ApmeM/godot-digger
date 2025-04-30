using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("StatueRight.tscn")]
public partial class StatueRight
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

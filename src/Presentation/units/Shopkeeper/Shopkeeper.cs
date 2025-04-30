using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Shopkeeper.tscn")]
public partial class Shopkeeper
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

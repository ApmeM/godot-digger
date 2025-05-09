using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Tree.tscn")]
public partial class Tree
{
    public Tree()
    {
        this.HP = 3;
        this.Loot = new List<string> { nameof(Wood) };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }
}

using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Tree2.tscn")]
public partial class Tree2
{
    public Tree2()
    {
        this.HP = 3;
        this.Loot = new List<string> { nameof(Wood) };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }
}

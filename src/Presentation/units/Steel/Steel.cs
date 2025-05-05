using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Steel.tscn")]
public partial class Steel
{
    public Steel()
    {
        this.HP = 3;
        this.Loot = new List<string> { nameof(Rock) };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }
}

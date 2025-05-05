using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("WoodPile.tscn")]
public partial class WoodPile
{
    public WoodPile()
    {
        this.HP = 2;
        this.Loot = new List<string> { nameof(Wood) };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }
}

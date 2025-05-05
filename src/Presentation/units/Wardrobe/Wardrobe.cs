using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Wardrobe.tscn")]
public partial class Wardrobe
{
    public Wardrobe()
    {
        this.HP = 4;
        this.Loot = new List<string> { nameof(Cloth) };
    }
    
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }
}

using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Grass.tscn")]
public partial class Grass
{
    public Grass()
    {
        this.HP = 1;
    }
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }
}

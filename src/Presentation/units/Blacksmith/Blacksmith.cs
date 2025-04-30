using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Blacksmith.tscn")]
public partial class Blacksmith
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

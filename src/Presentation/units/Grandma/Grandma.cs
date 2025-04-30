using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Grandma.tscn")]
public partial class Grandma
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

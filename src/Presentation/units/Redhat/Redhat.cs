using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Redhat.tscn")]
public partial class Redhat
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

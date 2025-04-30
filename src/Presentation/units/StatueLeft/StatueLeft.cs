using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("StatueLeft.tscn")]
public partial class StatueLeft
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

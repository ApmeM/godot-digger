using Godot;

[SceneReference("Forest.tscn")]
public partial class Forest
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

using Godot;

[SceneReference("Rock.tscn")]
public partial class Rock
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

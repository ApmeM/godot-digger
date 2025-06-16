using Godot;

[SceneReference("Stash.tscn")]
public partial class Stash
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

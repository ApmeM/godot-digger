using Godot;

[SceneReference("Sign.tscn")]
public partial class Sign
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

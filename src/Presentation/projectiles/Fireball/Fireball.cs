using Godot;

[SceneReference("Fireball.tscn")]
public partial class Fireball
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

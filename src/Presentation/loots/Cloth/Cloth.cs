using Godot;

[SceneReference("Cloth.tscn")]
public partial class Cloth
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

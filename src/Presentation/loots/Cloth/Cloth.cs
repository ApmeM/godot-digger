using Godot;

[SceneReference("Cloth.tscn")]
public partial class Cloth
{
    public Cloth()
    {
        Price = 20;
        MaxCount = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

using Godot;

[SceneReference("PlantViolet.tscn")]
public partial class PlantViolet
{
    public PlantViolet()
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

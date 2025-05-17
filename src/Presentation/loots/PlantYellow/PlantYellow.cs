using Godot;

[SceneReference("PlantYellow.tscn")]
public partial class PlantYellow
{
    public PlantYellow()
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

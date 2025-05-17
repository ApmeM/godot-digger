using Godot;

[SceneReference("PlantRed.tscn")]
public partial class PlantRed
{
    public PlantRed()
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

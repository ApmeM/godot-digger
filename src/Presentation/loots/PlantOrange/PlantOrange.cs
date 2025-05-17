using Godot;

[SceneReference("PlantOrange.tscn")]
public partial class PlantOrange
{
    public PlantOrange()
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

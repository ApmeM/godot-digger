using Godot;

[SceneReference("PlantBlue.tscn")]
public partial class PlantBlue
{
    public PlantBlue()
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

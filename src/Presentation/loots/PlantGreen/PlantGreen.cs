using Godot;

[SceneReference("PlantGreen.tscn")]
public partial class PlantGreen
{
    public PlantGreen()
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

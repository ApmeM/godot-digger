using Godot;

[SceneReference("PlantWhite.tscn")]
public partial class PlantWhite
{
    public PlantWhite()
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

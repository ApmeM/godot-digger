using Godot;

[SceneReference("PlantBrown.tscn")]
public partial class PlantBrown
{
    public PlantBrown()
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

using Godot;

[SceneReference("ChestCloth.tscn")]
public partial class ChestCloth
{
    public ChestCloth()
    {
        Price = 1;
        MaxCount = 1;
        ItemType = ItemType.Chest;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

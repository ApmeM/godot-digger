using Godot;

[SceneReference("Bag3.tscn")]
public partial class Bag3
{
    public Bag3()
    {
        EquipAction = (c) => c.SlotsCount += 3;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

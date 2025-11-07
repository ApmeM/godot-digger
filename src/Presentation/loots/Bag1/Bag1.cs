using Godot;

[SceneReference("Bag1.tscn")]
public partial class Bag1
{
    public Bag1()
    {
        EquipAction = (c) => c.SlotsCount += 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

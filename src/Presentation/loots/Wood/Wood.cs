using Godot;

[SceneReference("Wood.tscn")]
public partial class Wood
{
    public Wood()
    {
        EquipAction = (c) => c.AttackPower += 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

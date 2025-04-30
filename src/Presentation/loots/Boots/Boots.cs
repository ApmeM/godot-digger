using Godot;

[SceneReference("Boots.tscn")]
public partial class Boots
{
    public Boots()
    {
        Price = 1;
        MaxCount = 1;
        ItemType = ItemType.Boots;
        EquipAction = (c) => c.MaxStamina += 10;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

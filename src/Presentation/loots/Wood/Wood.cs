using Godot;

[SceneReference("Wood.tscn")]
public partial class Wood
{
    public Wood()
    {
        Price = 10;
        MaxCount = 1;
        ItemType = ItemType.Weapon;
        EquipAction = (c) => c.DigPower += 1;
        InventoryAction = (c) => c.EnemySpeedCoeff = 0.9f;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

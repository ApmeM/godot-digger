using Godot;

[SceneReference("WeaponPickaxe.tscn")]
public partial class WeaponPickaxe
{
    public WeaponPickaxe()
    {
        Price = 1;
        MaxCount = 1;
        ItemType = ItemType.Weapon;
        EquipAction = (c) => c.AttackPower += 2;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

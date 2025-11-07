using Godot;

[SceneReference("WeaponPickaxe.tscn")]
public partial class WeaponPickaxe
{
    public WeaponPickaxe()
    {
        EquipAction = (c) => c.AttackPower += 2;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

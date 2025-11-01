using Godot;

[SceneReference("Bag3.tscn")]
public partial class Bag3
{
    public Bag3()
    {
        Price = 1;
        MaxCount = 1;
        ItemType = ItemType.Bag;
        EquipAction = (c) => c.Inventory.Inventory.SlotsCount += 3;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

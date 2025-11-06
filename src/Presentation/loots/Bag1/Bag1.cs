using Godot;

[SceneReference("Bag1.tscn")]
public partial class Bag1
{
    public Bag1()
    {
        this.Price = 1; 
        this.MaxCount = 1; 
        this.ItemType = ItemType.Bag;
        EquipAction = (c) => c.SlotsCount += 3;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

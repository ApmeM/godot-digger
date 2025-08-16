using Godot;

[SceneReference("Gold.tscn")]
public partial class Gold
{
    public Gold()
    {
        this.Price = 1;
        this.MaxCount = 1000;
        this.ItemType = ItemType.Money;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

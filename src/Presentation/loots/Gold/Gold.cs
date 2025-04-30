using Godot;

[SceneReference("Gold.tscn")]
public partial class Gold
{
    public Gold()
    {
        this.Price = 1;
        this.MaxCount = 1000;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

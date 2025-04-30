using Godot;

[SceneReference("Rock.tscn")]
public partial class Rock
{
    public Rock()
    {
        this.Price = 30;
        this.MaxCount = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

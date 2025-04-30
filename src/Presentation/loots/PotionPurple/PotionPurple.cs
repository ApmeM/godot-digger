using Godot;

[SceneReference("PotionPurple.tscn")]
public partial class PotionPurple
{
    public PotionPurple()
    {
        this.Price = 1;
        this.MaxCount = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

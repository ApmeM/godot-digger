using Godot;

[SceneReference("PotionGreen.tscn")]
public partial class PotionGreen
{
    public PotionGreen()
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

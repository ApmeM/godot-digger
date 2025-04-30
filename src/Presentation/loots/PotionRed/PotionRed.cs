using Godot;

[SceneReference("PotionRed.tscn")]
public partial class PotionRed
{
    public PotionRed()
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

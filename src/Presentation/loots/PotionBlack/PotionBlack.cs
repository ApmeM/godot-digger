using Godot;

[SceneReference("PotionBlack.tscn")]
public partial class PotionBlack
{
    public PotionBlack()
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

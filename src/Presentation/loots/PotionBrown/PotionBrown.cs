using Godot;

[SceneReference("PotionBrown.tscn")]
public partial class PotionBrown
{
    public PotionBrown()
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

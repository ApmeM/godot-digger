using Godot;

[SceneReference("PotionViolet.tscn")]
public partial class PotionViolet
{
    public PotionViolet()
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

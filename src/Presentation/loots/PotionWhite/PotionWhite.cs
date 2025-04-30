using Godot;

[SceneReference("PotionWhite.tscn")]
public partial class PotionWhite
{
    public PotionWhite()
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

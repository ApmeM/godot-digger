using Godot;

[SceneReference("PotionGray.tscn")]
public partial class PotionGray
{
    public PotionGray()
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

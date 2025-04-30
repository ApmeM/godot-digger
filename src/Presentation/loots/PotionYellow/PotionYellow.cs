using Godot;

[SceneReference("PotionYellow.tscn")]
public partial class PotionYellow
{
    public PotionYellow()
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

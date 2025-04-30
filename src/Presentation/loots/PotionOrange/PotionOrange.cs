using Godot;

[SceneReference("PotionOrange.tscn")]
public partial class PotionOrange
{
    public PotionOrange()
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

using Godot;

[SceneReference("Statue.tscn")]
public partial class Statue
{
    private bool flipH;

    public bool FlipH
    {
        get => flipH;
        set
        {
            flipH = value;
            if (IsInsideTree())
            {
                this.animatedSprite.FlipH = value;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.FlipH = this.flipH;
    }
}

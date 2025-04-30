using Godot;

[SceneReference("WolfSkin.tscn")]
public partial class WolfSkin
{
    public WolfSkin()
    {
        Price = 50;
        MaxCount = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

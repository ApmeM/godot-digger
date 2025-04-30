using Godot;

[SceneReference("Bread.tscn")]
public partial class Bread
{
    public Bread()
    {
        Price = 30;
        MaxCount = 1;
        UseAction = (level) =>
        {
            level.HeaderControl.CurrentStamina += 5;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

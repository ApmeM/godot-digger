using Godot;

[SceneReference("StaminaPlant.tscn")]
public partial class StaminaPlant
{
    public StaminaPlant()
    {
        Price = 20;
        MaxCount = 1;
        UseAction = (level) =>
        {
            level.HeaderControl.CurrentStamina += 2;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

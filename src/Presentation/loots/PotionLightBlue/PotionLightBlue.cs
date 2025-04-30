using Godot;

[SceneReference("PotionLightBlue.tscn")]
public partial class PotionLightBlue
{
    public PotionLightBlue()
    {
        Price = 1;
        MaxCount = 1;
        UseAction = (level) =>
        {
            level.HeaderControl.CurrentStamina += 10;
        };
        ItemType = ItemType.Potion;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

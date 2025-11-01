using System.Threading.Tasks;
using Godot;

[SceneReference("PotionBlue.tscn")]
public partial class PotionBlue
{
    public PotionBlue()
    {
        Price = 1;
        MaxCount = 1;
        UseAction = (level) =>
        {
            level.CurrentLevel.HeaderControl.TrackingUnit.Buffs.AddBuff(nameof(StaminaRegen));
            level.CurrentLevel.HeaderControl.UpdateTrackingUnit();
            return Task.FromResult(true);
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

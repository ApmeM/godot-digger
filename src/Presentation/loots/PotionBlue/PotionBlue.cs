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
            level.HeaderControl.AddBuff(Buff.StaminaRegen);
            return Task.FromResult(true);
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

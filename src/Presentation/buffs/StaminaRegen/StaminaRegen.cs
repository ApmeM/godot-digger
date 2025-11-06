using Godot;

[SceneReference("StaminaRegen.tscn")]
public partial class StaminaRegen
{
    public StaminaRegen()
    {
        this.ApplyBuff = (character) =>
        {
            character.StaminaRecoverySeconds -= 15;
        };
    }
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

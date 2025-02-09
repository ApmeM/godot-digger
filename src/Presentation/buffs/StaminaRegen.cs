using Godot;

[SceneReference("StaminaRegen.tscn")]
public partial class StaminaRegen
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override string Description => "Stamina recovery increased.";

    public override void ApplyBuff(Character character)
    {
        character.StaminaRecoverySeconds -= 15;
    }
}

using Godot;

[SceneReference("SlowDown.tscn")]
public partial class SlowDown
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override string Description => "All units loose speed.";

    public override void ApplyBuff(Character character)
    {
        character.EnemySlowdownCoeff /= 2f;
    }
}

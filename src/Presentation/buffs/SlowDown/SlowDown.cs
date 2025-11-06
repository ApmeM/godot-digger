using Godot;

[SceneReference("SlowDown.tscn")]
public partial class SlowDown
{
    public SlowDown()
    {
        this.ApplyBuff = (character) =>
        {
            character.EnemySpeedCoeff /= 2f;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

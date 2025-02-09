using Godot;

[SceneReference("Dead.tscn")]
public partial class Dead
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void ApplyBuff(Character character)
    {
        character.CanDig = false;
    }
}

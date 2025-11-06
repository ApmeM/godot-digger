using Godot;

[SceneReference("Dead.tscn")]
public partial class Dead
{
    public Dead()
    {
        this.ApplyBuff = (character) =>
        {
            character.CanDig = false;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

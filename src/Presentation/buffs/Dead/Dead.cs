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
        this.RemoveBuff  = (character) =>
        {
            character.CanDig = true;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

using Godot;

[SceneReference("Dead.tscn")]
public partial class Dead
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override string Description => "You were near dead. \nNow you should rest. \nYou cant attack, dig or cut trees.";

    public override void ApplyBuff(Character character)
    {
        character.CanDig = false;
    }
}

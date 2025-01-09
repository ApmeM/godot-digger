using Godot;

[SceneReference("Level3.tscn")]
public partial class Level3
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
        if (pos == new Vector2(2, 2))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level2");
            return;
        }
        base.ChangeLevelClicked(pos);
    }
}

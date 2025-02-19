using Godot;

[SceneReference("Level2.tscn")]
public partial class Level2
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void CustomBlockClicked(Vector2 pos)
    {
        if (pos == new Vector2(8, 15))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level1");
            return;
        }
        if (pos == new Vector2(6, 1))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level3");
            return;
        }
        base.CustomBlockClicked(pos);
    }
}

using Godot;

[SceneReference("Woodcutter.tscn")]
public partial class Woodcutter
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void CustomConstructionClickedAsync(Vector2 pos)
    {
        if (pos == new Vector2(4, 9))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level1");
            return;
        }
        base.CustomConstructionClickedAsync(pos);
    }
}

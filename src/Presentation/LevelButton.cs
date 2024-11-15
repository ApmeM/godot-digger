using Godot;

[SceneReference("LevelButton.tscn")]
public partial class LevelButton
{
    [Export]
    public PackedScene dungeonScene;

    public string LevelName => this.dungeonScene.GetState().GetNodeName(0);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.AddToGroup(Groups.LevelButton);
    }
}

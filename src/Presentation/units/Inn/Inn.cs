using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Inn.tscn")]
public partial class Inn
{
    [Export]
    public string MoveToLevel { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(MoveClicked));
    }

    private void MoveClicked()
    {
        if (string.IsNullOrWhiteSpace(MoveToLevel))
        {
            return;
        }

        level.EmitSignal(nameof(BaseLevel.ChangeLevel), MoveToLevel);
    }
}

using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("StairsDown.tscn")]
public partial class StairsDown
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

        var level = this.GetNode<BaseLevel>(this.LevelPath);
        level.EmitSignal(nameof(BaseLevel.ChangeLevel), MoveToLevel);
    }
}

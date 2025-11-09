using Godot;
using System;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.screenTexture.Connect(CommonSignals.Pressed, this, nameof(ScreenTexturePressed));
    }

    public void ScreenTexturePressed()
    {
        this.EmitSignal(nameof(BaseLevel.ChangeLevel), nameof(Level2));
    }
}

using Godot;

[SceneReference("LevelMenu.tscn")]
public partial class LevelMenu
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.newGameButton.Connect(CommonSignals.Pressed, this, nameof(NewGamePressed));
        this.quickLoadButton.Connect(CommonSignals.Pressed, this, nameof(QuickLoadPressed));
    }

    public void DeferredGotoGame(string saveName)
    {
        this.GetParent<Game>().Load(saveName);
    }

    private void NewGamePressed()
    {
        this.EmitSignal(nameof(BaseLevel.ChangeLevel), nameof(Level1));
    }

    private void QuickLoadPressed()
    {
        CallDeferred(nameof(DeferredGotoGame), "quicksave");
    }
}

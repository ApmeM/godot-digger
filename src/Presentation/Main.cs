using System;
using Godot;


[SceneReference("Main.tscn")]
public partial class Main
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
        var root = this.GetTree().Root;

        // Immediately free the current scene, there is no risk here.
        var currentScene = root.GetChild(0);
        currentScene.QueueFree();
        root.RemoveChild(currentScene);

        // Load a new scene and add it to the active scene, as child of root.
        var nextScene = ResourceLoader.Load<PackedScene>("res://Presentation/Game.tscn").Instance<Game>();
        root.AddChild(nextScene);
        root.GetTree().CurrentScene = nextScene;
        nextScene.Load(saveName);
    }

    private void NewGamePressed()
    {
        this.GetTree().ChangeScene("res://Presentation/Game.tscn");
    }

    private void QuickLoadPressed()
    {
        CallDeferred(nameof(DeferredGotoGame), "quicksave");
    }
}

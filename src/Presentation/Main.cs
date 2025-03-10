using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Main.tscn")]
public partial class Main
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        ChangeLevel("Level1");
    }

    public void ChangeLevel(string nextLevel)
    {
        if (this.gamePosition.GetChildCount() > 0)
        {
            Save();
        }
        this.gamePosition.ClearChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var game = levelScene.Instance<BaseLevel>();
        game.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        this.gamePosition.AddChild(game);
        // game.Load();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && (KeyList)keyEvent.Scancode == KeyList.F2)
        {
            Save();
        }
    }

    public void Save()
    {
        ((BaseLevel)this.gamePosition.GetChild(0)).Save();
        ((BaseLevel)this.gamePosition.GetChild(0)).ShowPopup("Saved.");
    }
}

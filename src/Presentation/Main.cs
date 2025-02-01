using System.Collections.Generic;
using System.Linq;
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
        this.gamePosition.ClearChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var game = levelScene.Instance<BaseLevel>();
        game.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        this.gamePosition.AddChild(game);
    }
}

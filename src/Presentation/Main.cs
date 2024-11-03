using Godot;

[SceneReference("Main.tscn")]
public partial class Main
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        this.game.Visible = false;
        this.menu.Visible = true;

        this.game.Connect(nameof(Game.ExitDungeon), this, nameof(ExitDungeon));
        this.menu.Connect(nameof(Menu.LevelSelected), this, nameof(LevelSelected));
    }

    private void LevelSelected(PackedScene levelScene)
    {
        this.game.Visible = true;
        this.menu.Visible = false;
        this.game.InitMap(levelScene);
    }

    public void ExitDungeon(int stairsType, string fromLevel)
    {
        if ((Blocks)stairsType == Blocks.StairsUp)
        {
            this.game.Visible = false;
            this.menu.Visible = true;
            return;
        }
        var nextLevel = this.menu.GetNextLevel(stairsType, fromLevel);

        this.gameState.OpenLevel(nextLevel);
        this.menu.LoadLevel(nextLevel);
    }
}

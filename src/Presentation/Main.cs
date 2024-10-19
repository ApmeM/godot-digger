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
        this.gameState.NumberOfTurns = 10;

        this.game.Connect(nameof(Game.ExitDungeon), this, nameof(ExitDungeon));
        this.menu.Connect(nameof(Menu.LevelSelected), this, nameof(LevelSelected));
    }

    private void LevelSelected(int gameId)
    {
        this.game.Visible = true;
        this.menu.Visible = false;
        this.game.InitMap();
    }


    public void ExitDungeon()
    {
        this.game.Visible = false;
        this.menu.Visible = true;
    }
}

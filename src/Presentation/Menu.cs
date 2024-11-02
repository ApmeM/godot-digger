using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Menu.tscn")]
public partial class Menu
{
    [Signal]
    public delegate void LevelSelected(int levelId);
    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.ResourcesChanged), this, nameof(ResourcesChanged));
        this.gameState.Connect(nameof(GameState.LevelSelected), this, nameof(LevelPressed));
        this.achievements.Connect(CommonSignals.Pressed, this, nameof(AchievementsPressed));
        this.dungeon.Connect(CommonSignals.Pressed, this, nameof(DungeonPressed));
        this.sleep.Connect(CommonSignals.Pressed, this, nameof(SleepPressed));
        this.blacksmith.Connect(CommonSignals.Pressed, this, nameof(BlacksmithPressed));
        this.exit.Connect(CommonSignals.Pressed, this, nameof(ExitPressed));

        this.level1.Connect(CommonSignals.Pressed, this, nameof(LevelPressed), new Godot.Collections.Array { nameof(Level1) });
        this.level2.Connect(CommonSignals.Pressed, this, nameof(LevelPressed), new Godot.Collections.Array { nameof(Level2) });
    }

    private void ResourcesChanged()
    {
        this.woodCount.Text = this.gameState.GetResource(Loot.Wood).ToString();
        this.steelCount.Text = this.gameState.GetResource(Loot.Steel).ToString();
    }

    private void AchievementsPressed()
    {
        this.achievementList.ReloadList();
        this.windowDialog.PopupCentered();
    }

    private void DungeonPressed()
    {
        this.levelSelector.Visible = false;
        this.dungeonSelector.Visible = true;
    }

    private void ExitPressed()
    {
        this.levelSelector.Visible = true;
        this.dungeonSelector.Visible = false;
    }

    private void LevelPressed(string levelName)
    {
        this.DungeonPressed();
        this.EmitSignal(nameof(LevelSelected), levelName);
    }

    private void SleepPressed()
    {
        this.gameState.NumberOfTurns += 5;
    }

    private void BlacksmithPressed()
    {
        var irons = this.gameState.GetResource(Loot.Steel);
        if (irons >= 5 * this.gameState.DigPower)
        {
            this.gameState.DigPower++;
            this.gameState.AddResource(Loot.Steel, -5);
        }
    }
}

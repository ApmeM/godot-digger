using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Game.tscn")]
public partial class Game
{
    public int WoodCountValue
    {
        get => int.Parse(this.woodCount.Text);
        set => this.woodCount.Text = value.ToString();
    }

    [Signal]
    public delegate void ExitDungeon();

    public BaseLevel map;
    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.ResourcesChanged), this, nameof(ResourcesChanged));

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));
    }

    private void ResourcesChanged()
    {
        this.woodCount.Text = this.gameState.GetResource(Loot.Wood).ToString();
        this.steelCount.Text = this.gameState.GetResource(Loot.Steel).ToString();
    }

    private void VisibilityChanged()
    {
        this.header.Visible = this.Visible;
        this.mapHolder.Visible = this.Visible;
    }
    
    public void InitMap(string levelName)
    {
        this.map = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{levelName}.tscn").Instance<BaseLevel>();
        this.mapHolder.AddChild(this.map);
        this.map.Connect(nameof(BaseLevel.ExitCellClicked), this, nameof(ExitCellClicked));
    }

    private void ExitCellClicked()
    {
        this.mapHolder.ClearChildren();
        this.EmitSignal(nameof(ExitDungeon));
    }
}

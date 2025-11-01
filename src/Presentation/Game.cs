using Godot;
using Newtonsoft.Json;

[SceneReference("Game.tscn")]
public partial class Game
{
    private LevelData CurrentSave = new LevelData();

    public Header HeaderControl => this.header;
    public BaseLevel CurrentLevel => this.gamePosition.GetChildCount() > 0 ? this.gamePosition.GetChild<BaseLevel>(0) : null;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        ChangeLevel("Level2");

        this.quickSaveButton.Connect(CommonSignals.Pressed, this, nameof(QuickSaveClicked));
        this.menuButton.Connect(CommonSignals.Pressed, this, nameof(MenuButtonClicked));
    }

    private void QuickSaveClicked()
    {
        Save("quicksave");
    }

    private void MenuButtonClicked()
    {
        this.GetTree().ChangeScene("res://Presentation/Main.tscn");
    }

    public void GameOver()
    {
        if (this.IsInsideTree())
        {
            this.GetTree().ChangeScene("res://Presentation/Main.tscn");
        }
    }

    public void ChangeLevel(string nextLevel)
    {
        if (this.gamePosition.GetChildCount() > 0)
        {
            UpdateCurrentDump();
        }
        this.gamePosition.RemoveChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var level = levelScene.Instance<BaseLevel>();
        level.HeaderControl = this.header;
        level.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        level.Connect(nameof(BaseLevel.GameOver), this, nameof(GameOver));
        this.gamePosition.AddChild(level);
        CurrentSave.CurrentLevel = level.Name;
        LoadCurrentDump();
    }

    private void LoadCurrentDump()
    {
        CurrentLevel.LoadLevelDump(CurrentSave.Levels.ContainsKey(CurrentLevel.Name) ? CurrentSave.Levels[CurrentLevel.Name] : null);
    }

    private void UpdateCurrentDump()
    {
        CurrentSave.Levels[CurrentLevel.Name] = CurrentLevel.GetLevelDump();
    }

    public void Save(string name)
    {
        UpdateCurrentDump();

        var f = new File();
        f.Open($"user://SaveLevel_{name}.dat", File.ModeFlags.Write);
        f.StorePascalString(JsonConvert.SerializeObject(this.CurrentSave, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        }));
        f.Close();

        GD.Print("Saved.");
    }

    public void Load(string name)
    {
        this.gamePosition.RemoveChildren();
        var f = new File();
        if (!f.FileExists($"user://SaveLevel_{name}.dat"))
        {
            this.CurrentSave = new LevelData();
            ChangeLevel("Level1");
            return;
        }

        f.Open($"user://SaveLevel_{name}.dat", File.ModeFlags.Read);
        this.CurrentSave = JsonConvert.DeserializeObject<LevelData>(f.GetPascalString(), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        f.Close();

        ChangeLevel(this.CurrentSave.CurrentLevel);
        GD.Print("Loaded.");
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if ((KeyList)keyEvent.Scancode == KeyList.F2)
            {
                this.EmitSignal(nameof(Save), "quicksave");
            }

            if ((KeyList)keyEvent.Scancode == KeyList.F3)
            {
                this.EmitSignal(nameof(Load), "quicksave");
            }
        }
    }
}

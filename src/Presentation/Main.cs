using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("Main.tscn")]
public partial class Main
{
    private LevelData CurrentSave = new LevelData();

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
            UpdateCurrentDump();
        }
        this.gamePosition.RemoveChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var game = levelScene.Instance<BaseLevel>();
        game.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        this.gamePosition.AddChild(game);
        CurrentSave.CurrentLevel = game.Name;
        LoadCurrentDump();
    }

    private void LoadCurrentDump()
    {
        BaseLevel game = (BaseLevel)this.gamePosition.GetChild(0);
        game.LoadLevelDump(CurrentSave.Levels.ContainsKey(game.Name) ? CurrentSave.Levels[game.Name] : null);
        game.HeaderControl.LoadHeaderDump(CurrentSave.Header);
        game.LoadInventoryDump(CurrentSave.Inventory);
    }

    private void UpdateCurrentDump()
    {
        BaseLevel prevLevel = (BaseLevel)this.gamePosition.GetChild(0);
        CurrentSave.Levels[prevLevel.Name] = prevLevel.GetLevelDump();
        CurrentSave.Header = prevLevel.HeaderControl.GetHeaderDump();
        CurrentSave.Inventory = prevLevel.GetInventoryDump();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if ((KeyList)keyEvent.Scancode == KeyList.F2)
            {
                Save("quicksave");
            }

            if ((KeyList)keyEvent.Scancode == KeyList.F3)
            {
                Load("quicksave");
            }
        }
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

        ((BaseLevel)this.gamePosition.GetChild(0)).ShowPopup("Saved.");
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

        ((BaseLevel)this.gamePosition.GetChild(0)).ShowPopup("Loaded.");
    }
}

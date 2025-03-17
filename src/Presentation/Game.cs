using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("Game.tscn")]
public partial class Game
{
    private LevelData CurrentSave = new LevelData();


    public Header HeaderControl => this.header;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        ChangeLevel("Level1");

        this.header.Connect(nameof(Header.InventoryButtonClicked), this, nameof(ShowInventoryPopup));
        this.header.Connect(nameof(Header.BuffsChanged), this, nameof(BuffsChanged));
        this.header.Connect(nameof(Header.Save), this, nameof(Save));
        this.header.Connect(nameof(Header.Load), this, nameof(Load));
        this.bagInventoryPopup.Connect(nameof(BagInventoryPopup.BagChanged), this, nameof(BagChanged));
        this.bagInventoryPopup.Connect(nameof(BagInventoryPopup.EquipmentChanged), this, nameof(EquipmentChanged));
        this.bagInventoryPopup.Connect(nameof(BagInventoryPopup.UseItem), this, nameof(InventoryUseItem));
    }

    protected void InventoryUseItem(InventorySlot slot)
    {
        var tileId = slot.ItemId;
        if (LootDefinition.KnownLoot[(tileId, 0, 0)].UseAction != null)
        {
            LootDefinition.KnownLoot[(tileId, 0, 0)].UseAction(this);
            slot.TryChangeCount(slot.ItemId, -1);
        }
    }

    private void CharacteristicsChanged()
    {
        var character = new Character();
        this.bagInventoryPopup.ApplyEquipment(character);
        this.header.ApplyBuffs(character);

        this.header.Character = character;
        this.bagInventoryPopup.Size = character.BagSlots;
    }

    private void BuffsChanged()
    {
        this.CharacteristicsChanged();
    }

    private void EquipmentChanged(InventorySlot slot, int itemId, int from, int to)
    {
        this.CharacteristicsChanged();
    }

    private async void BagChanged(int itemId, int from, int to)
    {
        await this.ToSignal(GetTree().CreateTimer(0.01f), CommonSignals.Timeout); // Hack to update bag size after all signals handled.
        this.CharacteristicsChanged();
    }

    private void ShowInventoryPopup()
    {
        this.bagInventoryPopup.Show();
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
        game.HeaderControl = this.header;
        game.BagInventoryPopup = this.bagInventoryPopup;
        game.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        this.gamePosition.AddChild(game);
        CurrentSave.CurrentLevel = game.Name;
        LoadCurrentDump();

        CharacteristicsChanged();
    }

    private void LoadCurrentDump()
    {
        BaseLevel game = (BaseLevel)this.gamePosition.GetChild(0);
        game.LoadLevelDump(CurrentSave.Levels.ContainsKey(game.Name) ? CurrentSave.Levels[game.Name] : null);
        this.HeaderControl.LoadHeaderDump(CurrentSave.Header);
        this.bagInventoryPopup.LoadInventoryDump(CurrentSave.Inventory);
    }


    private void UpdateCurrentDump()
    {
        BaseLevel prevLevel = (BaseLevel)this.gamePosition.GetChild(0);
        CurrentSave.Levels[prevLevel.Name] = prevLevel.GetLevelDump();
        CurrentSave.Header = this.HeaderControl.GetHeaderDump();
        CurrentSave.Inventory = this.bagInventoryPopup.GetInventoryDump();
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

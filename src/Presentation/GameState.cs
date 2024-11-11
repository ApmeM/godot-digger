using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("GameState.tscn")]
public partial class GameState
{
    private class State
    {
        public readonly Dictionary<Loot, uint> resources = new Dictionary<Loot, uint>();
        public uint digPower;
        public uint numberOfTurnsMax;
        public uint numberOfTurns;
        public float numberOfTurnsRecoverySeconds;
        public DateTime numberOfTurnsLastUpdate;
        public readonly HashSet<string> openedLevels = new HashSet<string>();
        public string LevelName;
        public readonly List<(int, int)> LevelInventoryItems = new List<(int, int)>();
        public readonly List<(Vector2, uint, Vector2)> Floor = new List<(Vector2, uint, Vector2)>();
        public readonly List<(Vector2, uint, Vector2)> Blocks = new List<(Vector2, uint, Vector2)>();
        public readonly List<(Vector2, uint, Vector2)> Loot = new List<(Vector2, uint, Vector2)>();
        public readonly List<(Vector2, uint, Vector2)> Fog = new List<(Vector2, uint, Vector2)>();
    }

    [Signal]
    public delegate void NumberOfTurnsChanged();

    [Signal]
    public delegate void ResourcesChanged();

    [Signal]
    public delegate void OpenedLevelsChanged();

    [Signal]
    public delegate void LoadLevel(string levelName);

    private State state = new State();

    public void OpenLevel(string levelName)
    {
        this.state.openedLevels.Add(levelName);
        this.EmitSignal(nameof(OpenedLevelsChanged));
        this.SaveGame();
    }

    public bool IsLevelOpened(string levelName)
    {
        return this.state.openedLevels.Contains(levelName);
    }

    public uint GetResource(Loot resource)
    {
        return !state.resources.ContainsKey(resource) ? 0 : state.resources[resource];
    }

    public void AddResource(Loot resource, int diff)
    {
        this.state.resources[resource] = (uint)Math.Max(0, GetResource(resource) + diff);
        this.EmitSignal(nameof(ResourcesChanged));
        this.SaveGame();
    }

    public uint DigPower
    {
        get => state.digPower;
        set
        {
            this.state.digPower = value;
            this.SaveGame();
        }
    }

    public string LevelName
    {
        get => state.LevelName;
        set
        {
            this.state.LevelName = value;
            this.SaveGame();
        }
    }

    public uint NumberOfTurnsMax
    {
        get => state.numberOfTurnsMax;
        set
        {
            this.state.numberOfTurnsMax = value;
            this.SaveGame();
        }
    }

    public uint NumberOfTurns
    {
        get => state.numberOfTurns;
        set
        {
            if (this.state.numberOfTurns == this.NumberOfTurnsMax && value < this.NumberOfTurnsMax)
            {
                this.NumberOfTurnsLastUpdate = DateTime.Now;
            }

            this.state.numberOfTurns = value;
            if (this.state.numberOfTurns > this.NumberOfTurnsMax)
            {
                this.state.numberOfTurns = this.NumberOfTurnsMax;
            }
            this.EmitSignal(nameof(NumberOfTurnsChanged));
            this.SaveGame();
        }
    }

    public float NumberOfTurnsRecoverySeconds
    {
        get => state.numberOfTurnsRecoverySeconds;
        set
        {
            this.state.numberOfTurnsRecoverySeconds = value;
            this.SaveGame();
        }
    }

    public DateTime NumberOfTurnsLastUpdate
    {
        get => state.numberOfTurnsLastUpdate;
        set
        {
            this.state.numberOfTurnsLastUpdate = value;
            this.SaveGame();
        }
    }

    public void LoadMaps(TileMap floor, TileMap blocks, TileMap loot, TileMap fog)
    {
        if (
            this.state.Fog.Count == 0 &&
            this.state.Loot.Count == 0 &&
            this.state.Blocks.Count == 0 &&
            this.state.Floor.Count == 0
            )
        {
            return;
        }
        
        LoadTileMap(fog, this.state.Fog);
        LoadTileMap(loot, this.state.Loot);
        LoadTileMap(blocks, this.state.Blocks);
        LoadTileMap(floor, this.state.Floor);
    }
    private void LoadTileMap(TileMap level, List<(Vector2, uint, Vector2)> list)
    {
        level.Clear();
        foreach (var data in list)
        {
            level.SetCellv(data.Item1, (int)data.Item2, autotileCoord: data.Item3);
        }
    }

    public void SaveMaps(TileMap floor, TileMap blocks, TileMap loot, TileMap fog)
    {
        SaveTileMap(fog, this.state.Fog);
        SaveTileMap(loot, this.state.Loot);
        SaveTileMap(blocks, this.state.Blocks);
        SaveTileMap(floor, this.state.Floor);
    }

    private void SaveTileMap(TileMap level, List<(Vector2, uint, Vector2)> list)
    {
        list.Clear();
        var levelCells = level.GetUsedCells();
        foreach (Vector2 cell in levelCells)
        {
            list.Add((cell, (uint)level.GetCellv(cell), level.GetCellAutotileCoord((int)cell.x, (int)cell.y)));
        }
    }

    public void ClearMaps()
    {
        this.state.Fog.Clear();
        this.state.Loot.Clear();
        this.state.Blocks.Clear();
        this.state.Floor.Clear();
        this.SaveGame();
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.GetParent<Main>().Connect(CommonSignals.Ready, this, nameof(PostReady));
    }

    private void PostReady()
    {
        this.LoadGame();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (needSave)
        {
            needSave = false;

            var stringState = JsonConvert.SerializeObject(this.state);

            var saveGame = new File();
            saveGame.Open($"user://Savegame.save", File.ModeFlags.Write);
            saveGame.StoreLine(stringState);
            saveGame.Close();
            GD.Print($"State saved : {stringState}");
        }

        if (NumberOfTurns < NumberOfTurnsMax && NumberOfTurnsLastUpdate.AddSeconds(NumberOfTurnsRecoverySeconds) < DateTime.Now)
        {
            NumberOfTurns++;
            NumberOfTurnsLastUpdate = NumberOfTurnsLastUpdate.AddSeconds(NumberOfTurnsRecoverySeconds);
        }
    }

    public void AddLevelInventoryItem(int item, int count)
    {
        this.state.LevelInventoryItems.Add((item, count));
        SaveGame();
    }

    public List<(int, int)> GetLevelInventoryItems()
    {
        return this.state.LevelInventoryItems;
    }

    public void ClearLevelInventoryItems()
    {
        this.state.LevelInventoryItems.Clear();
        this.SaveGame();
    }

    private bool needSave = false;
    private void SaveGame()
    {
        this.needSave = true;
    }

    public void LoadGame()
    {
        var saveGame = new File();

        if (!saveGame.FileExists("user://Savegame.save"))
        {
            GD.Print("Save game file not exists.");

            this.state = new State
            {
                digPower = 1,
                numberOfTurnsMax = 10,
                numberOfTurns = 10,
                numberOfTurnsRecoverySeconds = 20,
                numberOfTurnsLastUpdate = DateTime.Now,
            };

            this.EmitSignal(nameof(ResourcesChanged));
            this.EmitSignal(nameof(NumberOfTurnsChanged));
            this.EmitSignal(nameof(OpenedLevelsChanged));

            GD.Print("State created.");
            this.SaveGame();
        }
        else
        {
            saveGame.Open($"user://Savegame.save", File.ModeFlags.Read);
            var stringState = saveGame.GetLine();
            saveGame.Close();
            GD.Print($"State loaded : {stringState}");
            this.state = JsonConvert.DeserializeObject<State>(stringState);
        }

        if (!string.IsNullOrWhiteSpace(this.state.LevelName))
        {
            this.EmitSignal(nameof(LoadLevel), this.state.LevelName);
        }
        this.EmitSignal(nameof(ResourcesChanged));
        this.EmitSignal(nameof(NumberOfTurnsChanged));
        this.EmitSignal(nameof(OpenedLevelsChanged));
    }

    public void MoveInventory()
    {
        foreach (var item in this.GetLevelInventoryItems())
        {
            this.AddResource((Loot)item.Item1, item.Item2);
        }
        this.ClearLevelInventoryItems();
        this.SaveGame();
    }
}

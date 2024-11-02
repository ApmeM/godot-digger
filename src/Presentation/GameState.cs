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

        if (NumberOfTurns >= NumberOfTurnsMax)
        {
            return;
        }

        if (NumberOfTurnsLastUpdate.AddSeconds(NumberOfTurnsRecoverySeconds) < DateTime.Now)
        {
            NumberOfTurns++;
            NumberOfTurnsLastUpdate = NumberOfTurnsLastUpdate.AddSeconds(NumberOfTurnsRecoverySeconds);
        }

        if (needSave)
        {
            needSave = false;

            var savedScenes = this.GetTree().GetNodesInGroup(Groups.SavedScene);
            if (savedScenes.Count > 0)
            {
                var scene = (BaseLevel)savedScenes[0];
                this.state.LevelName = scene.Name;
                scene.SaveFog(this.state.Fog);
                scene.SaveBlocks(this.state.Blocks);
                scene.SaveFloor(this.state.Floor);
                scene.SaveLoot(this.state.Loot);
            }
            else
            {
                this.state.LevelName = string.Empty;
                this.state.Fog.Clear();
                this.state.Blocks.Clear();
                this.state.Floor.Clear();
                this.state.Loot.Clear();
            }

            var stringState = JsonConvert.SerializeObject(this.state);

            var saveGame = new File();
            saveGame.Open($"user://Savegame.save", File.ModeFlags.Write);
            saveGame.StoreLine(stringState);
            saveGame.Close();
            GD.Print($"State saved : {stringState}");
        }
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

            return;
        }

        saveGame.Open($"user://Savegame.save", File.ModeFlags.Read);
        var stringState = saveGame.GetLine();
        GD.Print($"State loaded : {stringState}");

        this.state = JsonConvert.DeserializeObject<State>(stringState);
        if (!string.IsNullOrWhiteSpace(this.state.LevelName))
        {
            this.EmitSignal(nameof(LoadLevel), this.state.LevelName);
            var scene = (BaseLevel)this.GetTree().GetNodesInGroup(Groups.SavedScene)[0];
            scene.LoadFog(this.state.Fog);
            scene.LoadBlocks(this.state.Blocks);
            scene.LoadFloor(this.state.Floor);
            scene.LoadLoot(this.state.Loot);
        }
        saveGame.Close();


        this.EmitSignal(nameof(ResourcesChanged));
        this.EmitSignal(nameof(NumberOfTurnsChanged));
        this.EmitSignal(nameof(OpenedLevelsChanged));
    }
}

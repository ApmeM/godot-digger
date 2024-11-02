using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("GameState.tscn")]
public partial class GameState
{
    private class State
    {
        public Dictionary<Loot, uint> resources;
        public uint digPower;
        public uint numberOfTurnsMax;
        public uint numberOfTurns;
        public float numberOfTurnsRecoverySeconds;
        public DateTime numberOfTurnsLastUpdate;
        public HashSet<string> openedLevels;
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

            var stringState = JsonConvert.SerializeObject(this.state);

            var savedScenes = this.GetTree().GetNodesInGroup(Groups.SavedScene);

            var saveGame = new File();
            saveGame.Open($"user://Savegame.save", File.ModeFlags.Write);
            saveGame.StoreLine(stringState);
            saveGame.Store8((byte)(savedScenes.Count > 0 ? 1 : 0));
            if (savedScenes.Count > 0)
            {
                var currentLevel = (BaseLevel)savedScenes[0];
                saveGame.StoreLine(currentLevel.Name);
                currentLevel.Save(saveGame);
            }
            saveGame.Store8(0); // Last is always 0 to be possible to extend the savegame file
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

        if (saveGame.FileExists("user://Savegame.save"))
        {
            GD.Print("Save game file not exists.");
            
            this.state = new State
            {
                resources = new Dictionary<Loot, uint>(),
                digPower = 1,
                numberOfTurnsMax = 10,
                numberOfTurns = 10,
                numberOfTurnsRecoverySeconds = 20,
                numberOfTurnsLastUpdate = DateTime.Now,
                openedLevels = new HashSet<string>()
            };

            this.EmitSignal(nameof(ResourcesChanged));
            this.EmitSignal(nameof(NumberOfTurnsChanged));
            this.EmitSignal(nameof(OpenedLevelsChanged));

            GD.Print("State created");
            this.SaveGame();

            return;
        }

        saveGame.Open($"user://Savegame.save", File.ModeFlags.Read);
        var stringState = saveGame.GetLine();
        GD.Print($"State loaded : {stringState}");
        if (saveGame.Get8() > 0)
        {
            var levelName = saveGame.GetLine();
            this.EmitSignal(nameof(LoadLevel), levelName);
            var currentLevel = (BaseLevel)this.GetTree().GetNodesInGroup(Groups.SavedScene)[0];
            currentLevel.Load(saveGame);
        }
        saveGame.Close();

        this.state = JsonConvert.DeserializeObject<State>(stringState);

        this.EmitSignal(nameof(ResourcesChanged));
        this.EmitSignal(nameof(NumberOfTurnsChanged));
        this.EmitSignal(nameof(OpenedLevelsChanged));
    }
}

using System;
using System.Collections.Generic;
using Godot;
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
    }

    [Signal]
    public delegate void NumberOfTurnsChanged();

    [Signal]
    public delegate void ResourcesChanged();

    private State state = new State();

    public uint GetResource(Loot resource)
    {
        return !state.resources.ContainsKey(resource) ? 0 : state.resources[resource];
    }

    public void AddResource(Loot resource, int diff)
    {
        state.resources[resource] = (uint)Math.Max(0, GetResource(resource) + diff);
        this.EmitSignal(nameof(ResourcesChanged));
        SaveGame();
    }

    public uint DigPower
    {
        get => state.digPower;
        set
        {
            state.digPower = value;
            SaveGame();
        }
    }

    public uint NumberOfTurnsMax
    {
        get => state.numberOfTurnsMax;
        set
        {
            state.numberOfTurnsMax = value;
            SaveGame();
        }
    }

    public uint NumberOfTurns
    {
        get => state.numberOfTurns;
        set
        {
            if (this.state.numberOfTurns == this.NumberOfTurnsMax && value < this.NumberOfTurnsMax)
            {
                this.state.numberOfTurnsLastUpdate = DateTime.Now;
                GD.Print($"this.state.numberOfTurnsLastUpdate: {this.state.numberOfTurnsLastUpdate}");
            }

            this.state.numberOfTurns = value;
            if (this.state.numberOfTurns > this.NumberOfTurnsMax)
            {
                this.state.numberOfTurns = this.NumberOfTurnsMax;
            }
            GD.Print($"this.state.numberOfTurns: {this.state.numberOfTurns}");
            this.EmitSignal(nameof(NumberOfTurnsChanged));
            this.SaveGame();
        }
    }

    public float NumberOfTurnsRecoverySeconds
    {
        get => state.numberOfTurnsRecoverySeconds;
        set
        {
            state.numberOfTurnsRecoverySeconds = value;
            SaveGame();
        }
    }

    public DateTime NumberOfTurnsLastUpdate
    {
        get => state.numberOfTurnsLastUpdate;
        set
        {
            state.numberOfTurnsLastUpdate = value;
            SaveGame();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
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
    }

    public void SaveGame()
    {
        var stringState = JsonConvert.SerializeObject(this.state);

        var saveGame = new File();
        saveGame.Open($"user://Savegame.save", File.ModeFlags.Write);
        saveGame.StoreLine(stringState);
        saveGame.Close();
        GD.Print($"State saved: {stringState}");
    }

    public void LoadGame()
    {
        var saveGame = new File();

        if (!saveGame.FileExists("user://Savegame.save"))
        {
            GD.Print("Save game file not exists.");
            this.state = new State
            {
                resources = new Dictionary<Loot, uint>(),
                digPower = 1,
                numberOfTurnsMax = 10,
                numberOfTurns = 10,
                numberOfTurnsRecoverySeconds = 20,
                numberOfTurnsLastUpdate = DateTime.Now
            };

            this.EmitSignal(nameof(ResourcesChanged));
            this.EmitSignal(nameof(NumberOfTurnsChanged));

            GD.Print("State created");

            return;
        }

        saveGame.Open($"user://Savegame.save", File.ModeFlags.Read);
        var stringState = saveGame.GetLine();
        saveGame.Close();

        GD.Print($"State loaded: {stringState}");
        this.state = JsonConvert.DeserializeObject<State>(stringState);

        this.EmitSignal(nameof(ResourcesChanged));
        this.EmitSignal(nameof(NumberOfTurnsChanged));
    }
}

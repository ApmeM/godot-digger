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
        public uint inventorySlots;
    }

    [Signal]
    public delegate void ResourcesChanged();

    private State state = new State();

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

    public uint InventorySlots
    {
        get => state.inventorySlots;
        set
        {
            this.state.inventorySlots = value;
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
                inventorySlots = 3,
            };

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

        this.EmitSignal(nameof(ResourcesChanged));
    }
}

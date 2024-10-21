using System;
using System.Collections.Generic;
using Godot;

[SceneReference("GameState.tscn")]
public partial class GameState
{
    [Signal]
    public delegate void NumberOfTurnsChanged();

    private readonly Dictionary<Resources, int> Resources = new Dictionary<Resources, int>();

    public int GetResource(Resources resource)
    {
        return !Resources.ContainsKey(resource) ? 0 : Resources[resource];
    }

    public void AddResource(Resources resource, int diff)
    {
        Resources[resource] = Math.Max(0, GetResource(resource) + diff);
        this.EmitSignal(nameof(ResourcesChanged));
    }

    public int DigPower = 1;

    [Signal]
    public delegate void ResourcesChanged();

    public int NumberOfTurnsMax = 10;

    private int numberOfTurns;
    public int NumberOfTurns
    {
        get => numberOfTurns;
        set
        {
            numberOfTurns = value;
            if (numberOfTurns > NumberOfTurnsMax)
            {
                numberOfTurns = NumberOfTurnsMax;
            }
            this.EmitSignal(nameof(NumberOfTurnsChanged));
        }
    }

    public float NumberOfTurnsRecoveryTime = 60;

    public float NumberOfTurnsCurrentRecoveryTime = 0;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (NumberOfTurns >= NumberOfTurnsMax)
        {
            NumberOfTurnsCurrentRecoveryTime = 0;
            return;
        }

        NumberOfTurnsCurrentRecoveryTime += delta;
        if (NumberOfTurnsCurrentRecoveryTime >= NumberOfTurnsRecoveryTime)
        {
            NumberOfTurns++;
            NumberOfTurnsCurrentRecoveryTime -= NumberOfTurnsRecoveryTime;
        }
    }
}

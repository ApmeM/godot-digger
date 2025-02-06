using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Header.tscn")]
[Tool]
public partial class Header
{
    [Signal]
    public delegate void InventoryButtonClicked();

    [Export]
    public uint MaxNumberOfTurns = 10;

    private uint currentNumberOfTurns = 10;

    [Export]
    public uint CurrentNumberOfTurns
    {
        get => currentNumberOfTurns;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (this.currentNumberOfTurns == this.MaxNumberOfTurns && value < this.MaxNumberOfTurns)
            {
                this.NumberOfTurnsLastUpdate = DateTime.Now;
            }

            this.currentNumberOfTurns = value;
            if (this.currentNumberOfTurns > this.MaxNumberOfTurns)
            {
                this.currentNumberOfTurns = this.MaxNumberOfTurns;
            }
            if (IsInsideTree())
            {
                this.turnsCount.Text = this.CurrentNumberOfTurns.ToString();
            }
        }
    }

    [Export]
    public float NumberOfTurnsRecoverySeconds = 20;

    private DateTime NumberOfTurnsLastUpdate = DateTime.Now;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.CurrentNumberOfTurns = this.currentNumberOfTurns;

        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(OpenInventory));
    }

    private void OpenInventory()
    {
        this.EmitSignal(nameof(InventoryButtonClicked));
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (this.CurrentNumberOfTurns == this.MaxNumberOfTurns)
        {
            this.staminaProgress.Value = 0;
            this.NumberOfTurnsLastUpdate = DateTime.Now;
            return;
        }

        if (NumberOfTurnsLastUpdate.AddSeconds(NumberOfTurnsRecoverySeconds) < DateTime.Now)
        {
            CurrentNumberOfTurns++;
            NumberOfTurnsLastUpdate = NumberOfTurnsLastUpdate.AddSeconds(NumberOfTurnsRecoverySeconds);
        }

        this.staminaProgress.Value = (DateTime.Now - this.NumberOfTurnsLastUpdate).TotalSeconds * 100 / this.NumberOfTurnsRecoverySeconds;
    }
}

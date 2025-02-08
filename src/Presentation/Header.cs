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
    public uint MaxStamina = 10;

    private uint MaxHp = 100;

    private uint currentStamina = 10;

    [Export]
    public uint CurrentStamina
    {
        get => currentStamina;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (this.currentStamina == this.MaxStamina && value < this.MaxStamina)
            {
                this.staminaLastUpdate = DateTime.Now;
            }

            this.currentStamina = value;
            if (this.currentStamina > this.MaxStamina)
            {
                this.currentStamina = this.MaxStamina;
            }
            if (IsInsideTree())
            {
                this.staminaLabel.Text = this.CurrentStamina.ToString();
            }
        }
    }

    private uint currentHp = 100;

    [Export]
    public uint CurrentHp
    {
        get => currentHp;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (this.currentHp == this.MaxHp && value < this.MaxHp)
            {
                this.hpLastUpdate = DateTime.Now;
            }

            this.currentHp = value;
            if (this.currentHp > this.MaxHp)
            {
                this.currentHp = this.MaxHp;
            }

            if (IsInsideTree())
            {
                this.hpProgress.Value = this.currentHp;
                this.hpLabel.Text = this.currentHp.ToString();
            }
        }
    }

    [Export]
    public float StaminaRecoverySeconds = 20;

    private DateTime staminaLastUpdate = DateTime.Now;

    [Export]
    public float HpRecoverySeconds = 5;

    private DateTime hpLastUpdate = DateTime.Now;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.CurrentStamina = this.currentStamina;
        this.CurrentHp = this.currentHp;

        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(OpenInventory));
    }

    private void OpenInventory()
    {
        this.EmitSignal(nameof(InventoryButtonClicked));
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (this.CurrentStamina == this.MaxStamina)
        {
            this.staminaProgress.Value = 0;
            this.staminaLastUpdate = DateTime.Now;
        }
        else
        {
            if (staminaLastUpdate.AddSeconds(StaminaRecoverySeconds) < DateTime.Now)
            {
                CurrentStamina++;
                staminaLastUpdate = staminaLastUpdate.AddSeconds(StaminaRecoverySeconds);
            }

            this.staminaProgress.Value = (DateTime.Now - this.staminaLastUpdate).TotalSeconds * 100 / this.StaminaRecoverySeconds;
        }

        if (this.CurrentHp == this.MaxHp)
        {
            this.hpLastUpdate = DateTime.Now;
        }
        else
        {
            if (hpLastUpdate.AddSeconds(HpRecoverySeconds) < DateTime.Now)
            {
                CurrentHp++;
                hpLastUpdate = hpLastUpdate.AddSeconds(HpRecoverySeconds);
            }
        }
    }
}

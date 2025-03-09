using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;
using Newtonsoft.Json;

[SceneReference("Header.tscn")]
[Tool]
public partial class Header
{
    [Signal]
    public delegate void InventoryButtonClicked();

    [Signal]
    public delegate void BuffsChanged();

    [Signal]
    public delegate void BuffsClicked(string description);

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

            if (this.currentStamina == this.Character.MaxStamina && value < this.Character.MaxStamina)
            {
                this.staminaLastUpdate = DateTime.Now;
            }

            this.currentStamina = value;
            if (this.currentStamina > this.Character.MaxStamina)
            {
                this.currentStamina = this.Character.MaxStamina;
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

            if (this.currentHp == this.Character.MaxHp && value < this.Character.MaxHp)
            {
                this.hpLastUpdate = DateTime.Now;
            }

            this.currentHp = value;
            if (this.currentHp > this.Character.MaxHp)
            {
                this.currentHp = this.Character.MaxHp;
            }

            if (IsInsideTree())
            {
                this.hpProgress.Value = this.currentHp;
                this.hpLabel.Text = this.currentHp.ToString();
            }
        }
    }


    private DateTime staminaLastUpdate = DateTime.Now;

    private DateTime hpLastUpdate = DateTime.Now;

    public Character Character = new Character();

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

    public BaseBuff AddBuff(Buff buff)
    {
        var buffPath = $"res://Presentation/buffs/{buff}.tscn";
        var buffInstance = ResourceLoader.Load<PackedScene>(buffPath).Instance<BaseBuff>();
        buffInstance.Connect(nameof(BaseBuff.BuffRemoved), this, nameof(BuffRemoved), new Godot.Collections.Array { buffInstance });
        buffInstance.Connect(CommonSignals.Pressed, this, nameof(BuffClicked), new Godot.Collections.Array { buffInstance });
        this.buffContainer.AddChild(buffInstance);
        this.EmitSignal(nameof(BuffsChanged));
        return buffInstance;
    }

    private void BuffClicked(BaseBuff buff)
    {
        this.buffDescriptionLabel.Text = buff.Description;
        this.buffPopup.Show();
    }

    private void BuffRemoved(BaseBuff buff)
    {
        this.buffContainer.RemoveChild(buff);
        this.EmitSignal(nameof(BuffsChanged));
    }

    public void ApplyBuffs(Character character)
    {
        foreach (BaseBuff buff in this.buffContainer.GetChildren())
        {
            if (buff.IsQueuedForDeletion())
            {
                continue;
            }

            buff.ApplyBuff(character);
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (this.CurrentStamina == this.Character.MaxStamina)
        {
            this.staminaProgress.Value = 0;
            this.staminaLastUpdate = DateTime.Now;
        }
        else
        {
            if (staminaLastUpdate.AddSeconds(this.Character.StaminaRecoverySeconds) < DateTime.Now)
            {
                CurrentStamina++;
                staminaLastUpdate = staminaLastUpdate.AddSeconds(this.Character.StaminaRecoverySeconds);
            }

            this.staminaProgress.Value = (DateTime.Now - this.staminaLastUpdate).TotalSeconds * 100 / this.Character.StaminaRecoverySeconds;
        }

        if (this.CurrentHp == this.Character.MaxHp)
        {
            this.hpLastUpdate = DateTime.Now;
        }
        else
        {
            if (hpLastUpdate.AddSeconds(this.Character.HpRecoverySeconds) < DateTime.Now)
            {
                CurrentHp++;
                hpLastUpdate = hpLastUpdate.AddSeconds(this.Character.HpRecoverySeconds);
            }
        }
    }

    public void Save()
    {
        var f = new File();
        f.Open($"user://Header.dat", File.ModeFlags.Write);
        f.StorePascalString(JsonConvert.SerializeObject(this.CurrentHp));
        f.StorePascalString(JsonConvert.SerializeObject(this.hpLastUpdate));
        f.StorePascalString(JsonConvert.SerializeObject(this.CurrentStamina));
        f.StorePascalString(JsonConvert.SerializeObject(this.staminaLastUpdate));
        f.StorePascalString(JsonConvert.SerializeObject(this.buffContainer.GetChildren().Cast<BaseBuff>().Select(a => a.Name).ToList()));

        f.Close();
    }

    public void Load()
    {
        var f = new File();

        if (f.FileExists($"user://Header.dat"))
        {
            f.Open($"user://Header.dat", File.ModeFlags.Read);
            this.CurrentHp = JsonConvert.DeserializeObject<uint>(f.GetPascalString());
            this.hpLastUpdate = JsonConvert.DeserializeObject<DateTime>(f.GetPascalString());
            this.CurrentStamina = JsonConvert.DeserializeObject<uint>(f.GetPascalString());
            this.staminaLastUpdate = JsonConvert.DeserializeObject<DateTime>(f.GetPascalString());
            JsonConvert.DeserializeObject<List<string>>(f.GetPascalString()).ForEach(a => this.AddBuff((Buff)Enum.Parse(typeof(Buff), a)));
            f.Close();
        }
    }
}

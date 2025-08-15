using System;
using System.Linq;
using Godot;

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

    [Signal]
    public delegate void Save(string name);

    [Signal]
    public delegate void Load(string name);

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

    private Character character = new Character();
    public Character Character
    {
        get => character; set
        {
            character = value;

            this.inventoryBagItem.Visible = character.BagId != null;
            this.inventoryBagItem.Texture = character.BagId?.Image;

            this.staminaIconFront.Visible = character.WeaponId != null;
            this.staminaIconBack.Visible = character.WeaponId == null;

            this.staminaIconFront.Texture = character.WeaponId?.Image;
        }
    }
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.CurrentStamina = this.currentStamina;
        this.CurrentHp = this.currentHp;

        this.inventoryButton.Connect(CommonSignals.Pressed, this, nameof(OpenInventory));
        this.quickSaveButton.Connect(CommonSignals.Pressed, this, nameof(QuickSaveClicked));
        this.menuButton.Connect(CommonSignals.Pressed, this, nameof(MenuButtonClicked));
    }

    private void QuickSaveClicked()
    {
        this.EmitSignal(nameof(Save), "quicksave");
    }

    private void MenuButtonClicked()
    {
        this.GetTree().ChangeScene("res://Presentation/Main.tscn");
    }

    private void OpenInventory()
    {
        this.EmitSignal(nameof(InventoryButtonClicked));
    }

    public BaseBuff AddBuff(Buff buff)
    {
        return this.AddBuff(buff, 0);
    }

    public BaseBuff AddBuff(Buff buff, double progress)
    {
        var buffInstance = Instantiator.CreateBuff(buff);
        buffInstance.Connect(nameof(BaseBuff.BuffRemoved), this, nameof(BuffRemoved), new Godot.Collections.Array { buffInstance });
        buffInstance.Connect(CommonSignals.Pressed, this, nameof(BuffClicked), new Godot.Collections.Array { buffInstance });
        buffInstance.Progress = progress;
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
        buff.QueueFree();
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

    public HeaderDump GetHeaderDump()
    {
        return new HeaderDump
        {
            CurrentHP = this.CurrentHp,
            HPLastUpdate = this.hpLastUpdate,
            CurrentStaina = this.CurrentStamina,
            StaminaLastupdate = this.staminaLastUpdate,
            Buffs = this.buffContainer
                        .GetChildren()
                        .Cast<BaseBuff>()
                        .Select(a => new BuffDump
                        {
                            Name = a.Name,
                            Progress = a.Progress
                        }).ToList()
        };
    }

    public void LoadHeaderDump(HeaderDump header)
    {
        if (header != null)
        {
            this.CurrentHp = header.CurrentHP;
            this.hpLastUpdate = header.HPLastUpdate;
            this.CurrentStamina = header.CurrentStaina;
            this.staminaLastUpdate = header.StaminaLastupdate;
            header.Buffs.ForEach(a => this.AddBuff((Buff)Enum.Parse(typeof(Buff), a.Name)));
        }
        else
        {
            this.CurrentHp = 100;
            this.hpLastUpdate = DateTime.Now;
            this.CurrentStamina = 10;
            this.staminaLastUpdate = DateTime.Now;
            this.buffContainer.FreeChildren();
            this.EmitSignal(nameof(BuffsChanged));
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if ((KeyList)keyEvent.Scancode == KeyList.F2)
            {
                this.EmitSignal(nameof(Save), "quicksave");
            }

            if ((KeyList)keyEvent.Scancode == KeyList.F3)
            {
                this.EmitSignal(nameof(Load), "quicksave");
            }
        }
    }
}

using System;
using Godot;

[Tool]
[SceneReference("HP.tscn")]
public partial class HP
{
    private uint currentHP;
    private uint maxHP;

    [Export]
    public uint MaxHP
    {
        get => this.maxHP;
        set
        {
            this.maxHP = value;
            this.currentHP = Math.Min(this.currentHP, maxHP);
            if (IsInsideTree())
            {
                SetFrame();
            }
        }
    }

    [Export]
    public uint CurrentHP
    {
        get => this.currentHP;
        set
        {
            this.currentHP = value;
            this.currentHP = Math.Min(this.currentHP, maxHP);
            if (IsInsideTree())
            {
                SetFrame();
            }
        }
    }

    private void SetFrame()
    {
        if (this.MaxHP == 0)
        {
            this.Visible = false;
            return;
        }

        this.Frame = (int)(this.CurrentHP * 8 / this.MaxHP);
        this.Visible = this.currentHP != this.maxHP;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.MaxHP = this.maxHP;
        this.CurrentHP = this.currentHP;
    }
}

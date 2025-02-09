using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("BaseBuff.tscn")]
public partial class BaseBuff
{
    [Signal]
    public delegate void BuffRemoved();

    public virtual string Description
    {
        get
        {
            GD.PrintErr($"Buff descripio is not set for {this.GetType()}");
            return "Default description.";
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.timer.Connect(CommonSignals.Timeout, this, nameof(RemoveBuff));
    }

    private void RemoveBuff()
    {
        this.QueueFree();
        this.EmitSignal(nameof(BuffRemoved));
    }

    public virtual void ApplyBuff(Character character)
    {
    }

    private DateTime start = DateTime.MinValue;

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (start == DateTime.MinValue)
        {
            start = DateTime.Now;
        }

        this.textureProgress.Value = 100 - ((DateTime.Now - start).TotalSeconds * 100 / this.timer.WaitTime);
    }
}

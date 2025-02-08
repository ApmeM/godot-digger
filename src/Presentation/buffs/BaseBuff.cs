using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("BaseBuff.tscn")]
public partial class BaseBuff
{
    [Signal]
    public delegate void BuffRemoved();

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
}

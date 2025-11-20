using System;
using Godot;

[SceneReference("BaseBuff.tscn")]
public partial class BaseBuff
{
    [Export]
    public string Description { get; set; } = "Default description.";

    [Export]
    public float TotalTime { get; set; } = 1;

    [Export]
    public double Progress;

    public Action<BaseUnit.EffectiveCharacteristics> ApplyBuff { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        this.Progress += delta;

        this.textureProgress.Value = 100 * this.Progress / this.TotalTime;

        if (this.Progress >= this.TotalTime)
        {
            this.EmitSignal(nameof(BuffRemoved), this);
            this.QueueFree();
        }
    }

    #region Data

    public BuffDefinition BuffDefinition => BuffDefinition.BuffByName[this.GetType().Name];

    [Signal]
    public delegate void BuffRemoved(BaseBuff buff);

    #endregion
}

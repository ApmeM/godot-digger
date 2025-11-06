using System;
using Godot;

[SceneReference("BaseBuff.tscn")]
public partial class BaseBuff
{
    public BuffData buffData = new BuffData();

    [Export]
    public string Description { get; set; } = "Default description.";

    [Export]
    public float TotalTime { get; set; } = 1;

    public Action<BaseUnit.EffectiveCharacteristics> ApplyBuff { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        this.textureProgress.Value = 100 * this.buffData.Progress / this.buffData.BuffDefinition.TotalTime;
    }
}

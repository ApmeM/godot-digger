using System;
using System.Linq;
using Godot;

[SceneReference("BuffContainer.tscn")]
public partial class BuffContainer
{
    [Signal]
    public delegate void BuffClicked(BaseBuff buff);

    public override void _Ready()
    {
        base._Ready();

        foreach (BaseBuff buff in this.GetChildren())
        {
            if (!buff.IsConnected(CommonSignals.Pressed, this, nameof(BuffClickedhandler)))
            {
                buff.Connect(CommonSignals.Pressed, this, nameof(BuffClickedhandler), new Godot.Collections.Array { buff });
                buff.Connect(nameof(BaseBuff.BuffRemoved), this, nameof(BuffTimeout));
            }
        }
    }

    private void BuffClickedhandler(BaseBuff buff)
    {
        this.EmitSignal(nameof(BuffClicked), buff);
    }

    #region Data

    [Signal]
    public delegate void BuffsChanged();

    public BaseBuff AddBuff(string buffName)
    {
        var buffInstance = Instantiator.CreateBuff(buffName);
        buffInstance.Connect(CommonSignals.Pressed, this, nameof(BuffClicked), new Godot.Collections.Array { buffInstance });
        buffInstance.Connect(nameof(BaseBuff.BuffRemoved), this, nameof(BuffTimeout));
        this.AddChild(buffInstance);

        this.EmitSignal(nameof(BuffsChanged));

        return buffInstance;
    }

    private void BuffTimeout(BaseBuff buff)
    {
        this.RemoveChild(buff);
        this.EmitSignal(nameof(BuffsChanged));
    }

    public void RemoveBuff(BaseBuff buff)
    {
        buff.Progress = double.MaxValue;
    }

    public void Clear()
    {
        this.RemoveChildren();

        this.EmitSignal(nameof(BuffsChanged));
    }

    public void ApplyBuffs(BaseUnit.EffectiveCharacteristics character)
    {
        foreach (BaseBuff buff in this.GetChildren())
        {
            buff.BuffDefinition.ApplyBuff(character);
        }
    }

    #endregion
}

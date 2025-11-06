using System;
using System.Collections.Generic;
using System.Linq;

public class BuffsListData
{
    private List<BuffData> buffs = new List<BuffData>();
    public IEnumerable<BuffData> Buffs => this.buffs;

    public event Action BuffsChanged;

    public BuffData AddBuff(string buffName)
    {
        var buff = new BuffData
        {
            Name = buffName,
            Progress = 0,
        };
        buff.OnBuffRemoved += this.BuffTimeout;

        this.buffs.Add(buff);

        this.BuffsChanged?.Invoke();
        return buff;
    }

    private void BuffTimeout(BuffData buff)
    {
        buff.OnBuffRemoved -= BuffTimeout;
        this.buffs.Remove(buff);
        this.BuffsChanged?.Invoke();
    }

    public void RemoveBuff(BuffData buff)
    {
        buff.Progress = double.MaxValue;
    }

    public void Clear()
    {
        foreach (var buff in buffs)
        {
            buff.OnBuffRemoved -= BuffTimeout;
            buff.CallBuffRemoved();
        }

        this.buffs.Clear();
        this.BuffsChanged?.Invoke();
    }

    public void ApplyBuffs(BaseUnit.EffectiveCharacteristics character)
    {
        foreach (var buff in buffs)
        {
            buff.BuffDefinition.ApplyBuff(character);
        }
    }

    public void Tick(float delta)
    {
        foreach (var buff in buffs.ToList())
        {
            buff.Progress += delta;
        }
    }
}

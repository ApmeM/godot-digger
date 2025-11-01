using System;

public class BuffData
{
    public string Name;
    private double progress;

    public double Progress
    {
        get => progress;
        set
        {
            progress = value;
            if (progress >= BuffDefinition.TotalTime)
            {
                CallBuffRemoved();
            }
        }
    }

    public BuffDefinition BuffDefinition => BuffDefinition.BuffByName[this.Name];

    public event Action<BuffData> OnBuffRemoved;
    public void CallBuffRemoved()
    {
        OnBuffRemoved?.Invoke(this);
    }
}

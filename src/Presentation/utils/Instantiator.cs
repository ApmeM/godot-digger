using Godot;

public class Instantiator
{
    public static BaseBuff CreateBuff(Buff buff)
    {
        var path = $"res://Presentation/buffs/{buff}/{buff}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene.Instance<BaseBuff>();
    }

    internal static BaseLoot CreateLoot(string loot)
    {
        var path = $"res://Presentation/loots/{loot}/{loot}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene.Instance<BaseLoot>();
    }

    internal static BaseUnit CreateUnit(string unit)
    {
        var path = $"res://Presentation/units/{unit}/{unit}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene.Instance<BaseUnit>();
    }
}
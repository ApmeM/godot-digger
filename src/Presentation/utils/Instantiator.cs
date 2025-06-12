using Godot;

public class Instantiator
{
    public static PackedScene LoadBuff(Buff buff)
    {
        var path = $"res://Presentation/buffs/{buff}/{buff}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene;
    }

    internal static PackedScene LoadLoot(string loot)
    {
        var path = $"res://Presentation/loots/{loot}/{loot}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene;
    }

    internal static PackedScene LoadeUnit(string unit)
    {
        var path = $"res://Presentation/units/{unit}/{unit}.tscn";
        var scene = ResourceLoader.Load<PackedScene>(path);
        return scene;
    }
    public static BaseBuff CreateBuff(Buff buff)
    {
        return LoadBuff(buff).Instance<BaseBuff>();
    }

    internal static BaseLoot CreateLoot(string loot)
    {
        return LoadLoot(loot).Instance<BaseLoot>();
    }

    internal static BaseUnit CreateUnit(string unit)
    {
        return LoadeUnit(unit).Instance<BaseUnit>();
    }
}

using System;
using System.Collections.Generic;
using Godot;

public class BuffDump
{
    public string Name;
    public double Progress;
}

public class HeaderDump
{
    public uint CurrentHP;
    public DateTime HPLastUpdate;
    public uint CurrentStaina;
    public DateTime StaminaLastupdate;
    public List<BuffDump> Buffs;
}

public class LevelDump
{
    public List<(Vector2 a, int)> Floor;
    public List<(Vector2 a, int)> Constructions;
    public List<(Vector2 a, int)> Loot;
    public List<(Vector2 a, int)> Blocks;
    public List<(Vector2 a, int)> Fog;
    public List<KeyValuePair<Vector2, BlocksDefinition>> Meta;
    public object CustomData;
}

public class InventoryDump
{
    public (int, int) Bag;
    public List<(int, int)> Equipment;
    public List<(int, int)> Inventory;
}

public class LevelData
{
    public Dictionary<string, LevelDump> Levels = new Dictionary<string, LevelDump>();
    public HeaderDump Header;
    public InventoryDump Inventory;
    public string CurrentLevel;
}
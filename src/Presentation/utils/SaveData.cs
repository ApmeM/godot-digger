using System.Collections.Generic;
using Godot;

public class UnitDump
{
    public List<BuffData> Buffs;
    public BagInventoryData Inventory;
}

public class LevelDump
{
    public List<(Vector2 a, int)> Floor;
    public object CustomData;
    public Vector2 CameraZoom;
    public Vector2 CameraPos;
    public UnitDump Units;
}

public class LevelData
{
    public Dictionary<string, LevelDump> Levels = new Dictionary<string, LevelDump>();
    public string CurrentLevel;
}
using System.Collections.Generic;

public enum Floor
{
    Tiles,
    Wall,
    Ground,
}

public class FloorDefinition
{
    public static Dictionary<Floor, FloorDefinition> KnownFloors = new Dictionary<Floor, FloorDefinition>{
        { Floor.Wall, new FloorDefinition() },
        { Floor.Tiles, new FloorDefinition() },
        { Floor.Ground, new FloorDefinition() },
    };
}

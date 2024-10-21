using System.Collections.Generic;
using Godot;

public enum Resources
{
    Iron
}

public class CellDefinition
{
    public static Vector2 Path = new Vector2(1, 0);
    public static Vector2 Wood = new Vector2(13, 0);
    public static Vector2 Wall = new Vector2(4, 0);
    public static Vector2 Stairs = new Vector2(7, 0);
    public static Vector2 Iron = new Vector2(9, 0);

    public static Dictionary<Vector2, CellDefinition> KnownCells = new Dictionary<Vector2, CellDefinition>{
        { Path, new CellDefinition{HP = 0} },
        { Wood, new CellDefinition{HP = 1} },
        { Wall, new CellDefinition{HP = 0} },
        { Stairs, new CellDefinition{HP = 0} },
        { Iron, new CellDefinition{HP = 2, Resource = Resources.Iron, ResourceCount=1} },
    };

    public int HP;
    public Resources Resource;
    public int ResourceCount;

    public CellDefinition Clone()
    {
        return new CellDefinition
        {
            HP = this.HP,
            Resource = this.Resource,
            ResourceCount = this.ResourceCount,
        };
    }
}

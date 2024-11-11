using System.Collections.Generic;

public enum Fog
{
    Basic
}

public enum Blocks
{
    StairsUp,
    StairsDown,
    Wood,
    Steel,
    Wardrobe
}

public enum Loot
{
    Wood,
    Steel,
    Cloth
}

public enum Path
{
    Path,
    Wall
}

public class CellDefinition
{
    public static Dictionary<Blocks, CellDefinition> KnownBlocks = new Dictionary<Blocks, CellDefinition>{
        { Blocks.Wood, new CellDefinition{HP = 1} },
        { Blocks.Steel, new CellDefinition{HP = 2} },
        { Blocks.Wardrobe, new CellDefinition{HP = 4} },
    };

    public uint HP;

    public CellDefinition Clone()
    {
        return new CellDefinition
        {
            HP = this.HP,
        };
    }
}

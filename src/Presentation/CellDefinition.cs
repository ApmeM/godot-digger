using System.Collections.Generic;
using Godot;

public enum Fog
{
    Basic
}

public enum Blocks
{
    StairsUp,
    StairsDown,
    Wood,
    Steel
}

public enum Loot
{
    Wood,
    Steel
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
    };

    public int HP;

    public CellDefinition Clone()
    {
        return new CellDefinition
        {
            HP = this.HP,
        };
    }
}

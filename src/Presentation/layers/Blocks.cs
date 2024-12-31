using System.Collections.Generic;

public enum Blocks
{
    Wood,
    Steel,
    Wardrobe,
    Grass
}

public class BlocksDefinition
{
    public static Dictionary<Blocks, BlocksDefinition> KnownBlocks = new Dictionary<Blocks, BlocksDefinition>{
        { Blocks.Wood, new BlocksDefinition{HP = 2} },
        { Blocks.Steel, new BlocksDefinition{HP = 3} },
        { Blocks.Wardrobe, new BlocksDefinition{HP = 4} },
        { Blocks.Grass, new BlocksDefinition{HP = 1} },
    };

    public uint HP;

    public BlocksDefinition Clone()
    {
        return new BlocksDefinition
        {
            HP = this.HP,
        };
    }
}

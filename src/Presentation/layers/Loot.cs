using System;
using System.Collections.Generic;

public enum Loot
{
    Wood,
    Steel,
    Cloth,
    StaminaPlant
}

public class LootDefinition
{
    public static Dictionary<Loot, LootDefinition> KnownLoot = new Dictionary<Loot, LootDefinition>{
        { Loot.Wood, new LootDefinition{} },
        { Loot.Steel, new LootDefinition{} },
        { Loot.Cloth, new LootDefinition{} },
        { Loot.StaminaPlant, new LootDefinition{UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns += 2; }} },
    };

    public Action<BaseLevel> UseAction;
}

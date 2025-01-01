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
        { Loot.Wood, new LootDefinition{UseAction = (level)=>{ }} },
        { Loot.Steel, new LootDefinition{UseAction = (level)=>{}} },
        { Loot.Cloth, new LootDefinition{UseAction = (level)=>{}} },
        { Loot.StaminaPlant, new LootDefinition{UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns += 2; }} },
    };

    public Action<BaseLevel> UseAction;
}

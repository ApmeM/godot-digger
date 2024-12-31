using System;
using System.Collections.Generic;

public enum Loot
{
    Wood,
    Steel,
    Cloth
}

public class LootDefinition
{
    public static Dictionary<Loot, LootDefinition> KnownLoot = new Dictionary<Loot, LootDefinition>{
        { Loot.Wood, new LootDefinition{UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns = level.Stamina.MaxNumberOfTurns; }} },
        { Loot.Steel, new LootDefinition{UseAction = (level)=>{}} },
        { Loot.Cloth, new LootDefinition{UseAction = (level)=>{}} },
    };

    public Action<BaseLevel> UseAction;
}

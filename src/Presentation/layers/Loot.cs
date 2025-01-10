using System;
using System.Collections.Generic;
using Godot;

public static class Loot
{
    public static int Wood = 0;
    public static int Steel = 1;
    public static int Cloth = 2;
    public static int StaminaPlant = 3;
}

public class LootDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };
    private static Action<BaseLevel, Vector2> PutToBag = (level, pos) => { level.TryGrabLoot(pos); };

    public static Dictionary<int, LootDefinition> KnownLoot = new Dictionary<int, LootDefinition>{
        { Loot.Wood, new LootDefinition{ClickAction = PutToBag} },
        { Loot.Steel, new LootDefinition{ClickAction = PutToBag} },
        { Loot.Cloth, new LootDefinition{ClickAction = PutToBag} },
        { Loot.StaminaPlant, new LootDefinition{ClickAction = PutToBag, UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns += 2; }} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
    public Action<BaseLevel> UseAction { get; set; }
}

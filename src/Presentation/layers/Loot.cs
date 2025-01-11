using System;
using System.Collections.Generic;
using Godot;

public static class Loot
{
    public static ValueTuple<int, int, int> Wood = (0,0,0);
    public static ValueTuple<int, int, int> Steel = (0,1,0);
    public static ValueTuple<int, int, int> Cloth = (0,2,0);
    public static ValueTuple<int, int, int> StaminaPlant = (0,3,0);
}

public class LootDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> DoNothing = (level, pos) => { };
    private static Action<BaseLevel, Vector2> PutToBag = (level, pos) => { level.TryGrabLoot(pos); };

    public static Dictionary<ValueTuple<int, int, int>, LootDefinition> KnownLoot = new Dictionary<ValueTuple<int, int, int>, LootDefinition>{
        { Loot.Wood, new LootDefinition{ClickAction = PutToBag} },
        { Loot.Steel, new LootDefinition{ClickAction = PutToBag} },
        { Loot.Cloth, new LootDefinition{ClickAction = PutToBag} },
        { Loot.StaminaPlant, new LootDefinition{ClickAction = PutToBag, UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns += 2; }} },
    };

    public Action<BaseLevel, Vector2> ClickAction { get; set; }
    public Action<BaseLevel> UseAction { get; set; }
}

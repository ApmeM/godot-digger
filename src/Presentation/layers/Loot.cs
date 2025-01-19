using System;
using System.Collections.Generic;
using Godot;

public static class Loot
{
    public static ValueTuple<int, int, int> Wood = (6, 0, 0);
    public static ValueTuple<int, int, int> Steel = (5, 0, 0);
    public static ValueTuple<int, int, int> Cloth = (2, 0, 0);
    public static ValueTuple<int, int, int> StaminaPlant = (3, 0, 0);
    public static ValueTuple<int, int, int> Bread = (1, 0, 0);
    public static ValueTuple<int, int, int> WolfSkin = (4, 0, 0);
}

public class LootDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> CustomLoot = (level, pos) => { level.CustomLootClicked(pos); };

    public static Dictionary<ValueTuple<int, int, int>, LootDefinition> KnownLoot = new Dictionary<ValueTuple<int, int, int>, LootDefinition>{
        { Loot.Wood, new LootDefinition{Price=10, ClickAction = CustomLoot} },
        { Loot.Steel, new LootDefinition{Price=30, ClickAction = CustomLoot} },
        { Loot.Cloth, new LootDefinition{Price=20, ClickAction = CustomLoot} },
        { Loot.StaminaPlant, new LootDefinition{Price=20, ClickAction = CustomLoot, UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns += 2; }} },
        { Loot.Bread, new LootDefinition{Price=30, ClickAction = CustomLoot, UseAction = (level)=>{ level.Stamina.CurrentNumberOfTurns += 10; }} },
        { Loot.WolfSkin, new LootDefinition{Price=50, ClickAction = CustomLoot} },
    };

    public int Price { get; set; }
    public Action<BaseLevel, Vector2> ClickAction { get; set; }
    public Action<BaseLevel> UseAction { get; set; }
}

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
    public static ValueTuple<int, int, int> Gold = (7, 0, 0);
    public static ValueTuple<int, int, int> PotionEmpty = (8, 0, 0);
    public static ValueTuple<int, int, int> PotionBlack = (9, 0, 0);
    public static ValueTuple<int, int, int> PotionBlue = (10, 0, 0);
    public static ValueTuple<int, int, int> PotionBrown = (11, 0, 0);
    public static ValueTuple<int, int, int> PotionGray = (12, 0, 0);
    public static ValueTuple<int, int, int> PotionGreen = (13, 0, 0);
    public static ValueTuple<int, int, int> PotionLightBlue = (14, 0, 0);
    public static ValueTuple<int, int, int> PotionOrange = (15, 0, 0);
    public static ValueTuple<int, int, int> PotionPurple = (16, 0, 0);
    public static ValueTuple<int, int, int> PotionRed = (17, 0, 0);
    public static ValueTuple<int, int, int> PotionViolet = (18, 0, 0);
    public static ValueTuple<int, int, int> PotionWhite = (19, 0, 0);
    public static ValueTuple<int, int, int> PotionYellow = (20, 0, 0);
    public static ValueTuple<int, int, int> WeaponPickaxe = (21, 0, 0);
    public static ValueTuple<int, int, int> ChestCloth = (22, 0, 0);
    public static ValueTuple<int, int, int> Boots = (23, 0, 0);
}

public enum ItemType
{
    Neck = 1,
    Helm = 2,
    Weapon = 3,
    Chest = 4,
    Shield = 5,
    Ring = 6,
    Belt = 7,
    Pants = 8,
    Boots = 9,
    Potion = 10,
}

public class LootDefinition : IActionDefinition
{
    private static Action<BaseLevel, Vector2> CustomLoot = (level, pos) => { level.CustomLootClickedAsync(pos); };

    public static Dictionary<ValueTuple<int, int, int>, LootDefinition> KnownLoot = new Dictionary<ValueTuple<int, int, int>, LootDefinition>{
        { Loot.Wood, new LootDefinition{Price=10, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.Steel, new LootDefinition{Price=30, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.Cloth, new LootDefinition{Price=20, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.StaminaPlant, new LootDefinition{Price=20, MaxCount = 1, ClickAction = CustomLoot, UseAction = (level)=>{ level.Stamina.CurrentStamina += 2; }} },
        { Loot.Bread, new LootDefinition{Price=30, MaxCount = 1, ClickAction = CustomLoot, UseAction = (level)=>{ level.Stamina.CurrentStamina += 5; }} },
        { Loot.WolfSkin, new LootDefinition{Price=50, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.Gold, new LootDefinition{Price=1, MaxCount = 1000, ClickAction = CustomLoot} },
        { Loot.PotionEmpty, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot, MergeActions = new Dictionary<(int, int, int), (int, int, int)>{{Loot.StaminaPlant, Loot.PotionLightBlue}} }},
        { Loot.PotionBlack, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionBlue, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionBrown, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionGray, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionGreen, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionLightBlue, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot, UseAction = (level)=>{ level.Stamina.CurrentStamina += 10; }, ItemType = ItemType.Potion} },
        { Loot.PotionOrange, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionPurple, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionRed, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionViolet, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionWhite, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.PotionYellow, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot} },
        { Loot.WeaponPickaxe, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot, ItemType = ItemType.Weapon, DigPower = 2} },
        { Loot.ChestCloth, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot, ItemType = ItemType.Chest} },
        { Loot.Boots, new LootDefinition{Price=1, MaxCount = 1, ClickAction = CustomLoot, ItemType = ItemType.Boots, NumberOfTurns = 10} },
    };

    public Dictionary<ValueTuple<int, int, int>, ValueTuple<int, int, int>> MergeActions = new Dictionary<(int, int, int), (int, int, int)>();

    public int MaxCount;
    public int DigPower;
    public int NumberOfTurns;

    public ItemType ItemType { get; set; }

    public uint Price { get; set; }
    public Action<BaseLevel, Vector2> ClickAction { get; set; }
    public Action<BaseLevel> UseAction { get; set; }
}

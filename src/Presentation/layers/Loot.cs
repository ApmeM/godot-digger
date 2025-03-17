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
    public static ValueTuple<int, int, int> Bag3 = (24, 0, 0);
    public static ValueTuple<int, int, int> Bag1 = (25, 0, 0);
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
    Bag = 11,
}

public class LootDefinition
{
    public static Dictionary<ValueTuple<int, int, int>, LootDefinition> KnownLoot = new Dictionary<ValueTuple<int, int, int>, LootDefinition>{
        { Loot.Wood,            new LootDefinition{Price=10, MaxCount = 1} },
        { Loot.Steel,           new LootDefinition{Price=30, MaxCount = 1} },
        { Loot.Cloth,           new LootDefinition{Price=20, MaxCount = 1} },
        { Loot.StaminaPlant,    new LootDefinition{Price=20, MaxCount = 1, UseAction = (level)=>{ level.HeaderControl.CurrentStamina += 2; }} },
        { Loot.Bread,           new LootDefinition{Price=30, MaxCount = 1, UseAction = (level)=>{ level.HeaderControl.CurrentStamina += 5; }} },
        { Loot.WolfSkin,        new LootDefinition{Price=50, MaxCount = 1} },
        { Loot.Gold,            new LootDefinition{Price=1,  MaxCount = 1000} },
        { Loot.PotionEmpty,     new LootDefinition{Price=1,  MaxCount = 1, MergeActions = new Dictionary<(int, int, int), (int, int, int)>{{Loot.StaminaPlant, Loot.PotionLightBlue}} }},
        { Loot.PotionBlack,     new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionBlue,      new LootDefinition{Price=1,  MaxCount = 1, UseAction = (level)=>{level.HeaderControl.AddBuff(Buff.StaminaRegen);} } },
        { Loot.PotionBrown,     new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionGray,      new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionGreen,     new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionLightBlue, new LootDefinition{Price=1,  MaxCount = 1, UseAction = (level)=>{ level.HeaderControl.CurrentStamina += 10; }, ItemType = ItemType.Potion} },
        { Loot.PotionOrange,    new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionPurple,    new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionRed,       new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionViolet,    new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionWhite,     new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.PotionYellow,    new LootDefinition{Price=1,  MaxCount = 1} },
        { Loot.WeaponPickaxe,   new LootDefinition{Price=1,  MaxCount = 1, ItemType = ItemType.Weapon, DigPower = 2} },
        { Loot.ChestCloth,      new LootDefinition{Price=1,  MaxCount = 1, ItemType = ItemType.Chest} },
        { Loot.Boots,           new LootDefinition{Price=1,  MaxCount = 1, ItemType = ItemType.Boots, NumberOfTurns = 10} },
        { Loot.Bag3,            new LootDefinition{Price=1,  MaxCount = 1, ItemType = ItemType.Bag, AdditionalSlots = 3} },
        { Loot.Bag1,            new LootDefinition{Price=1,  MaxCount = 1, ItemType = ItemType.Bag, AdditionalSlots = 1} },
    };

    public Dictionary<ValueTuple<int, int, int>, ValueTuple<int, int, int>> MergeActions = new Dictionary<(int, int, int), (int, int, int)>();

    public int AdditionalSlots;
    public int MaxCount;
    public int DigPower;
    public int NumberOfTurns;

    public ItemType ItemType { get; set; }

    public uint Price { get; set; }
    public Action<Game> UseAction { get; set; }
}

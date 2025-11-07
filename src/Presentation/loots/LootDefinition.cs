using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class LootDefinition
{
    private static Dictionary<int, LootDefinition> lootById;
    private static Dictionary<string, LootDefinition> lootByName;
    public static Dictionary<int, LootDefinition> LootById
    {
        get
        {
            EnsureLoaded();
            return lootById;
        }
    }

    public static Dictionary<string, LootDefinition> LootByName
    {
        get
        {
            EnsureLoaded();
            return lootByName;
        }
    }

    public static void EnsureLoaded()
    {
        if (lootByName != null && lootById != null)
        {
            return;
        }

        var resByName = new Dictionary<string, LootDefinition>();
        var resById = new Dictionary<int, LootDefinition>();

        var dir = new Directory();
        dir.Open("res://Presentation/loots/");
        dir.ListDirBegin(true, true);
        string dirname;
        var id = 0;
        while (!string.IsNullOrWhiteSpace(dirname = dir.GetNext()))
        {
            if (!dir.FileExists($"res://Presentation/loots/{dirname}/{dirname}.png.import"))
            {
                continue;
            }

            id++;
            var texture = ResourceLoader.Load<Texture>($"res://Presentation/loots/{dirname}/{dirname}.png");
            var scene = Instantiator.CreateLoot(dirname);
            var definition = new LootDefinition
            {
                Image = texture,
                Id = id,
                Name = scene.LootName,
                Description = scene.LootDescription,
                ItemType = scene.ItemType,
                MaxCount = scene.MaxCount,
                MergeActions = scene.MergeActions,
                Price = scene.Price,
                UseAction = scene.UseAction,
                EquipAction = scene.EquipAction,
                InventoryAction = scene.InventoryAction
            };
            resByName.Add(dirname, definition);
            resById.Add(id, definition);
        }
        dir.ListDirEnd();

        lootByName = resByName;
        lootById = resById;
    }

    public int Id;
    public Texture Image;
    public string Name;
    public string Description;
    public int MaxCount;
    public ItemType ItemType { get; set; }
    public uint Price { get; set; }

    public Dictionary<string, string> MergeActions = new Dictionary<string, string>();
    public Func<Game, Task<bool>> UseAction { get; set; }
    public Action<BaseUnit.EffectiveCharacteristics> InventoryAction { get; set; }
    public Action<BaseUnit.EffectiveCharacteristics> EquipAction { get; set; }
}

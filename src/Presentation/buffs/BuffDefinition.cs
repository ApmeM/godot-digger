using System;
using System.Collections.Generic;
using Godot;

public class BuffDefinition
{
    private static Dictionary<int, BuffDefinition> buffById;
    private static Dictionary<string, BuffDefinition> buffByName;
    public static Dictionary<int, BuffDefinition> BuffById
    {
        get
        {
            EnsureLoaded();
            return buffById;
        }
    }

    public static Dictionary<string, BuffDefinition> BuffByName
    {
        get
        {
            EnsureLoaded();
            return buffByName;
        }
    }

    public static void EnsureLoaded()
    {
        if (buffByName != null && buffById != null)
        {
            return;
        }

        var resByName = new Dictionary<string, BuffDefinition>();
        var resById = new Dictionary<int, BuffDefinition>();

        var dir = new Directory();
        dir.Open("res://Presentation/buffs/");
        dir.ListDirBegin(true, true);
        string dirname;
        var id = 0;
        while (!string.IsNullOrWhiteSpace(dirname = dir.GetNext()))
        {
            if (!dir.FileExists($"res://Presentation/buffs/{dirname}/{dirname}.png.import"))
            {
                continue;
            }

            id++;
            var texture = ResourceLoader.Load<Texture>($"res://Presentation/buffs/{dirname}/{dirname}.png");
            var scene = Instantiator.CreateBuff(dirname);
            var definition = new BuffDefinition
            {
                Image = texture,
                Id = id,
                Name = dirname,
                Description = scene.Description,
                ApplyBuff = scene.ApplyBuff,
                TotalTime = scene.TotalTime
            };
            resByName.Add(dirname, definition);
            resById.Add(id, definition);
        }
        dir.ListDirEnd();

        buffByName = resByName;
        buffById = resById;
    }

    public int Id;
    public Texture Image;
    public string Name;
    public string Description;
    public double TotalTime;
    public Action<BaseUnit.EffectiveCharacteristics> ApplyBuff;
}

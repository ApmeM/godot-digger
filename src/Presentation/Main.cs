using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Main.tscn")]
public partial class Main
{
    [Export]
    public Texture LootTexture;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        var number = LootTexture.GetWidth() / 48;
        for (var i = 0; i < number; i++)
        {
            var lootItem = new AtlasTexture
            {
                Atlas = LootTexture,
                Region = new Rect2(i * 48, 0, 48, 48)
            };

            this.inventory.Resources.Add(lootItem);
            ChangeLevel("Level1", new List<Loot>());
        }
    }

    public void ChangeLevel(string nextLevel, List<Loot> resources)
    {
        this.ResourcesAdded(resources);

        this.gamePosition.ClearChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var game = levelScene.Instance<BaseLevel>();
        game.Resources = this.inventory.Resources;
        game.InitMap(this.MaxNumberOfTurns, this.InventorySlots, this.DigPower);
        game.Connect(nameof(BaseLevel.ChangeLevel), this, nameof(ChangeLevel));
        this.gamePosition.AddChild(game);
    }

    [Export]
    public uint InventorySlots = 3;

    [Export]
    public uint DigPower = 1;

    [Export]
    public uint MaxNumberOfTurns = 10;

    public void ResourcesAdded(List<Loot> newResources)
    {
        foreach (var res in newResources)
        {
            this.inventory.TryAddItem((int)res, 1);
        }
    }
}

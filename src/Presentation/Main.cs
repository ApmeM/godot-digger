using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Main.tscn")]
public partial class Main
{
    [Export]
    public Texture LootTexture;
    private List<Texture> resources;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // For debug purposes all achievements can be reset
        // this.di.localAchievementRepository.ResetAchievements();

        var number = LootTexture.GetWidth() / 48;
        this.resources = Enumerable.Range(0, number)
            .Select(i => new AtlasTexture { Atlas = LootTexture, Region = new Rect2(i * 48, 0, 48, 48) })
            .Cast<Texture>()
            .ToList();

        ChangeLevel("Level1");
    }

    public void ChangeLevel(string nextLevel)
    {
        this.gamePosition.ClearChildren();
        var levelScene = ResourceLoader.Load<PackedScene>($"res://Presentation/levels/{nextLevel}.tscn");
        var game = levelScene.Instance<BaseLevel>();
        game.Resources = this.resources;
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
}

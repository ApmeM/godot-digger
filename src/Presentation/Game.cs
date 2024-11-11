using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Game.tscn")]
public partial class Game
{
    [Signal]
    public delegate void ExitDungeon(int stairsType, string fromLevel);

    public BaseLevel map;

    [Export]
    public Texture LootTexture;

    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");
        this.gameState = this.GetNode<GameState>("/root/Main/GameState");

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));
        this.inventory.Connect(CommonSignals.Pressed, this, nameof(ShowInventoryPopup));
        var number = LootTexture.GetWidth() / 16;
        for (var i = 0; i < number; i++)
        {
            this.customPopupInventory.Resources.Add(new AtlasTexture
            {
                Atlas = LootTexture,
                Region = new Rect2(i * 16, 0, 16, 16)
            });
        }
    }

    private void ShowInventoryPopup()
    {
        this.customPopupInventory.ShowAt(this.inventory.RectPosition / this.customPopupInventory.Scale);
    }

    private void VisibilityChanged()
    {
        this.header.Visible = this.Visible;
        this.mapHolder.Visible = this.Visible;
    }

    public BaseLevel InitMap(PackedScene levelScene)
    {
        this.map = levelScene.Instance<BaseLevel>();
        this.mapHolder.AddChild(this.map);

        this.map.LoadGame();

        foreach (var item in this.gameState.GetLevelInventoryItems())
        {
            this.customPopupInventory.TryAddItem(item.Item1, item.Item2);
        }

        this.map.Connect(nameof(BaseLevel.ExitCellClicked), this, nameof(ExitCellClicked));
        return this.map;
    }

    private void ExitCellClicked(int stairsType)
    {
        this.gameState.LevelName = string.Empty;
        this.gameState.ClearMaps();
        this.gameState.MoveInventory();
        this.customPopupInventory.ClearItems();
        this.mapHolder.ClearChildren();
        this.EmitSignal(nameof(ExitDungeon), stairsType, this.map.Name);
        this.map = null;
    }

    public bool TryAddResource(Loot item, int count)
    {
        var result = this.customPopupInventory.TryAddItem((int)item, count);
        if (result)
        {
            this.gameState.AddLevelInventoryItem((int)item, count);
        }

        return result;
    }
}

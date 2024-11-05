using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Game.tscn")]
public partial class Game
{
    [Signal]
    public delegate void ExitDungeon(int stairsType, string fromLevel);

    public BaseLevel map;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

        this.Connect(CommonSignals.VisibilityChanged, this, nameof(VisibilityChanged));
        this.inventory.Connect(CommonSignals.Pressed, this, nameof(ShowInventoryPopup));
    }

    private void ShowInventoryPopup()
    {
        this.customPopupInventory.Show();
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
        this.map.Connect(nameof(BaseLevel.ExitCellClicked), this, nameof(ExitCellClicked));
        return this.map;
    }

    private void ExitCellClicked(int stairsType)
    {
        this.mapHolder.ClearChildren();
        this.EmitSignal(nameof(ExitDungeon), stairsType, this.map.Name);
        this.map = null;
    }

    internal bool TryAddResource(Loot item, int count)
    {
        return this.customPopupInventory.TryAddItem((int)item, count);
    }
}

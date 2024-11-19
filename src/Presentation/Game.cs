using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Game.tscn")]
public partial class Game
{
    [Signal]
    public delegate void ExitDungeon(int stairsType, string fromLevel, List<Loot> resources);

    public BaseLevel map;

    [Export]
    public Texture LootTexture;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        // this.achievementNotifications.UnlockAchievement("MyFirstAchievement");

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

    public BaseLevel InitMap(PackedScene levelScene, uint maxNumberOfTurns, uint inventorySlots, uint digPower)
    {
        this.customPopupInventory.Size = inventorySlots;

        this.stamina.MaxNumberOfTurns = maxNumberOfTurns;
        this.stamina.CurrentNumberOfTurns = this.stamina.MaxNumberOfTurns;

        this.map = levelScene.Instance<BaseLevel>();
        this.map.CanDig = true;
        this.map.DigPower = digPower;
        this.map.Connect(nameof(BaseLevel.ExitCellClicked), this, nameof(ExitCellClicked));
        this.map.Connect(nameof(BaseLevel.DigCellClicked), this, nameof(DigCellClicked));

        this.mapHolder.AddChild(this.map);

        return this.map;
    }

    private void DigCellClicked()
    {
        this.stamina.CurrentNumberOfTurns--;
        this.map.CanDig = this.stamina.CurrentNumberOfTurns > 0;
    }

    private void ExitCellClicked(int stairsType)
    {
        var oldMap = this.map;
        this.map = null;
        this.mapHolder.ClearChildren();
        var resources = this.customPopupInventory.GetItems().Select(a => (Loot)a.Item1).ToList();
        this.customPopupInventory.ClearItems();
        this.EmitSignal(nameof(ExitDungeon), stairsType, oldMap.Name, resources);
    }

    public bool TryAddResource(Loot item, int count)
    {
        return this.customPopupInventory.TryAddItem((int)item, count);
    }
}

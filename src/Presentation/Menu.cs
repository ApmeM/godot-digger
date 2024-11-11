using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Menu.tscn")]
public partial class Menu
{
    [Signal]
    public delegate void LevelSelected(PackedScene levelScene);

    [Export]
    public Texture LootTexture;

    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.ResourcesChanged), this, nameof(ResourcesChanged));
        this.gameState.Connect(nameof(GameState.LoadLevel), this, nameof(LoadLevel));
        this.gameState.Connect(nameof(GameState.OpenedLevelsChanged), this, nameof(OpenedLevelsChanged));
        this.achievements.Connect(CommonSignals.Pressed, this, nameof(AchievementsPressed));
        this.dungeon.Connect(CommonSignals.Pressed, this, nameof(DungeonPressed));
        this.sleep.Connect(CommonSignals.Pressed, this, nameof(SleepPressed));
        this.blacksmith.Connect(CommonSignals.Pressed, this, nameof(BlacksmithPressed));
        this.leather.Connect(CommonSignals.Pressed, this, nameof(LeatherPressed));
        this.exit.Connect(CommonSignals.Pressed, this, nameof(ExitPressed));
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

        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            level.Connect(CommonSignals.Pressed, this, nameof(LevelPressed), new Godot.Collections.Array { level.dungeonScene });
        }
    }

    private void ShowInventoryPopup()
    {
        this.customPopupInventory.ShowAt(this.inventory.RectPosition / this.customPopupInventory.Scale);
    }

    private void OpenedLevelsChanged()
    {
        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            level.Disabled = !this.gameState.IsLevelOpened(level.LevelName);
        }

        // First level alvays visible
        this.level1.Disabled = false;
    }

    private void ResourcesChanged()
    {
        this.customPopupInventory.ClearItems();
        foreach (Loot item in Enum.GetValues(typeof(Loot)))
        {
            this.customPopupInventory.TryAddItem((int)item, (int)this.gameState.GetResource(item));
        }
    }

    private void AchievementsPressed()
    {
        this.achievementList.ReloadList();
        this.windowDialog.PopupCentered();
    }

    private void DungeonPressed()
    {
        this.levelSelector.Visible = false;
        this.dungeonSelector.Visible = true;
    }

    private void ExitPressed()
    {
        this.levelSelector.Visible = true;
        this.dungeonSelector.Visible = false;
    }

    private void LevelPressed(PackedScene levelScene)
    {
        this.DungeonPressed();
        this.EmitSignal(nameof(LevelSelected), levelScene);
    }

    public void LoadLevel(string levelName)
    {
        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            if (level.LevelName == levelName)
            {
                LevelPressed(level.dungeonScene);
                return;
            }
        }
    }

    private void SleepPressed()
    {
        this.gameState.NumberOfTurns += 5;
    }

    private async void BlacksmithPressed()
    {
        var res = Loot.Cloth;
        var irons = this.gameState.GetResource(res);
        var required = Fibonacci.Calc(this.gameState.DigPower);
        if (irons >= required)
        {
            this.customConfirmPopup.ContentText = $"Increase pickaxe power?\nIt requires {required} irons.";
            this.customConfirmPopup.ShowCentered();
            var decision = (bool)(await ToSignal(this.customConfirmPopup, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
            if (decision)
            {
                this.gameState.AddResource(res, -required);
                this.gameState.DigPower++;
            }
        }
        else
        {
            this.customTextPopup.ContentText = $"Not enough iron.\n{required} irons required.";
            this.customTextPopup.ShowCentered();
        }
    }

    private async void LeatherPressed()
    {
        var res = Loot.Cloth;
        var cloth = this.gameState.GetResource(res);
        var required = Fibonacci.Calc(this.gameState.InventorySlots);
        if (cloth >= required)
        {
            this.customConfirmPopup.ContentText = $"Increase number of inventory slots?\nIt requires {required} cloth.";
            this.customConfirmPopup.ShowCentered();
            var decision = (bool)(await ToSignal(this.customConfirmPopup, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
            if (decision)
            {
                this.gameState.AddResource(res, -required);
                this.gameState.InventorySlots++;
            }
        }
        else
        {
            this.customTextPopup.ContentText = $"Not enough cloth.\n{required} cloth required.";
            this.customTextPopup.ShowCentered();
        }
    }

    public string GetNextLevel(int stairsType, string fromLevel)
    {
        switch (fromLevel)
        {
            case "Level1":
                return "Level2";
            default:
                throw new Exception("Victory!!!");
        }
    }
}

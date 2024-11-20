using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Menu.tscn")]
public partial class Menu
{
    [Signal]
    public delegate void LevelSelected(PackedScene levelScene);

    [Export]
    public Texture LootTexture;

    [Export]
    public uint InventorySlots = 3;

    [Export]
    public uint DigPower = 1;

    [Export]
    public uint MaxNumberOfTurns = 10;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.achievements.Connect(CommonSignals.Pressed, this, nameof(AchievementsPressed));
        this.dungeon.Connect(CommonSignals.Pressed, this, nameof(DungeonPressed));
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

        foreach (var child in this.GetTree().GetNodesInGroup(Groups.LevelButton))
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

    public void ResourcesAdded(List<Loot> newResources)
    {
        foreach (var res in newResources)
        {
            this.customPopupInventory.TryAddItem((int)res, 1);
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

    public void OpenLevel(string levelName)
    {
        foreach (var child in this.dungeonSelector.GetChildren())
        {
            if (!(child is LevelButton level))
            {
                continue;
            }

            if (level.LevelName == levelName)
            {
                level.Visible = true;
                return;
            }
        }
    }

    private void BlacksmithPressed()
    {
        SpendResource(Loot.Steel, "irons", Fibonacci.Calc(this.DigPower + 5), () => this.DigPower++);
    }

    private void LeatherPressed()
    {
        SpendResource(Loot.Cloth, "cloth", Fibonacci.Calc(this.InventorySlots), () => this.InventorySlots++);
    }

    private async void SpendResource(Loot res, string resName, uint required, Action action)
    {
        var existing = this.customPopupInventory.GetItemCount((int)res);
        if (existing >= required)
        {
            this.customConfirmPopup.ContentText = $"Increase pickaxe power?\nIt requires {required} {resName}.";
            this.customConfirmPopup.ShowCentered();
            var decision = (bool)(await ToSignal(this.customConfirmPopup, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
            if (decision)
            {
                existing = this.customPopupInventory.GetItemCount((int)res);
                if (existing >= required)
                {
                    this.customPopupInventory.TryRemoveItems((int)res, required);
                    action();
                }
            }
        }
        else
        {
            this.customTextPopup.ContentText = $"Not enough {resName}.\n{required} {resName} required.";
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

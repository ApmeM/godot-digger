using System;
using System.Collections.Generic;
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

    [Export]
    public readonly Dictionary<Loot, int> Resources = new Dictionary<Loot, int>();

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
        foreach (Loot item in newResources)
        {
            this.Resources[item] = !this.Resources.ContainsKey(item) ? 1 : this.Resources[item] + 1;
        }
        this.customPopupInventory.ClearItems();
        foreach (var res in this.Resources)
        {
            this.customPopupInventory.TryAddItem((int)res.Key, res.Value);
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

    private async void BlacksmithPressed()
    {
        var res = Loot.Cloth;
        var irons = this.Resources[res];
        var required = Fibonacci.Calc(this.DigPower + 5);
        if (irons >= required)
        {
            this.customConfirmPopup.ContentText = $"Increase pickaxe power?\nIt requires {required} irons.";
            this.customConfirmPopup.ShowCentered();
            var decision = (bool)(await ToSignal(this.customConfirmPopup, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
            if (decision)
            {
                this.Resources[res] = !this.Resources.ContainsKey(res) ? 0 : this.Resources[res] - required;
                this.DigPower++;
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
        var cloth = this.Resources[res];
        var required = Fibonacci.Calc(this.InventorySlots);
        if (cloth >= required)
        {
            this.customConfirmPopup.ContentText = $"Increase number of inventory slots?\nIt requires {required} cloth.";
            this.customConfirmPopup.ShowCentered();
            var decision = (bool)(await ToSignal(this.customConfirmPopup, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
            if (decision)
            {
                this.Resources[res] = !this.Resources.ContainsKey(res) ? 0 : this.Resources[res] - required;
                this.InventorySlots++;
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

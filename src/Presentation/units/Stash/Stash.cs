using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Stash.tscn")]
public partial class Stash
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Connect(CommonSignals.Pressed, this, nameof(StashPressed));

        foreach (var id in LootDefinition.LootByName)
        {
            this.stashInventory.Config.Add((int)id.Value.Id, new InventorySlot.InventorySlotConfig
            {
                Texture = id.Value.Image,
                MaxCount = id.Value.MaxCount * 10,
                ItemType = (int)id.Value.ItemType
            });
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        this.stashInventory.QueueFree();
    }
    
    private async void StashPressed()
    {
        var level = this.GetNode<BaseLevel>(this.LevelPath);
        level.BagInventoryPopup.ConfigureInventory(this.stashInventory);

        this.stashInventory.Visible = true;
        level.BagInventoryPopup.Show();
        await this.ToSignal(level.BagInventoryPopup, nameof(CustomPopup.PopupClosed));
        this.stashInventory.Visible = false;
    }

    // ToDo: Save stash data
    // public class CustomData
    // {
    //     public List<(int, int)> Stash;
    // }

    // public override LevelDump GetLevelDump()
    // {
    //     var dump = base.GetLevelDump();
    //     dump.CustomData = new CustomData
    //     {
    //         Stash = this.stashInventory.GetItems()
    //     };
    //     return dump;
    // }

    // public override void LoadLevelDump(LevelDump levelDump)
    // {
    //     base.LoadLevelDump(levelDump);

    //     if (levelDump == null)
    //     {
    //         return;
    //     }

    //     var customData = (CustomData)levelDump.CustomData;
    //     if (customData != null)
    //     {
    //         this.stashInventory.SetItems(customData.Stash);
    //     }
    //     else
    //     {
    //         this.stashInventory.ClearItems();
    //     }
    // }
}

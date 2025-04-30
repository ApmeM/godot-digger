using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("BaseLoot.tscn")]
public partial class BaseLoot
{

    [Export]
    public NodePath LevelPath;

    public string LootName => this.GetType().Name;

    public Dictionary<string, string> MergeActions = new Dictionary<string, string>();

    public int MaxCount;
    
    public ItemType ItemType { get; set; }

    public uint Price { get; set; }
    public Action<Game> UseAction { get; set; }
    public Action<Character> EquipAction { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Loot);

        this.Connect(CommonSignals.Pressed, this, nameof(LootClicked));
    }

    private void LootClicked()
    {
        var level = this.GetNode<BaseLevel>(LevelPath);
        if (level.BagInventoryPopup.TryChangeCount(this.LootName, 1) == 0)
        {
            this.QueueFree();
        }
    }
}

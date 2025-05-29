using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public Func<Game, Task<bool>> UseAction { get; set; }
    public Action<Character> EquipAction { get; set; }

    private BaseLevel internalLevel;
    protected BaseLevel level
    {
        get
        {
            internalLevel = internalLevel ?? this.GetNode<BaseLevel>(LevelPath);
            return internalLevel;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Loot);

        this.texture.Connect(CommonSignals.Pressed, this, nameof(LootClicked));
    }

    private void LootClicked()
    {
        if (level.BagInventoryPopup.TryChangeCount(this.LootName, 1) == 0)
        {
            this.QueueFree();
        }
    }
}

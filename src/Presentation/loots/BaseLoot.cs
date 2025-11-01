using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

[SceneReference("BaseLoot.tscn")]
public partial class BaseLoot
{
    [Signal]
    public delegate void LootClicked();

    public string LootName => this.GetType().Name;

    public Dictionary<string, string> MergeActions = new Dictionary<string, string>();

    public int MaxCount;
    
    public ItemType ItemType { get; set; }

    public uint Price { get; set; }
    public Func<Game, Task<bool>> UseAction { get; set; }
    public Action<BaseUnit> EquipAction { get; set; }
    public Action<BaseUnit> InventoryAction { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Loot);

        this.texture.Connect(CommonSignals.Pressed, this, nameof(LootTextureClicked));
    }

    private void LootTextureClicked()
    {
        this.EmitSignal(nameof(LootClicked));
    }
}

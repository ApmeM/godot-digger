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

    [Export]
    public string LootDescription = "Default loot description.";

    [Export]
    public int MaxCount;

    [Export]
    public ItemType ItemType { get; set; }

    [Export]
    public uint Price { get; set; }

    public Dictionary<string, string> MergeActions = new Dictionary<string, string>();
    public Func<Game, Task<bool>> UseAction { get; set; }
    public Action<BaseUnit.EffectiveCharacteristics> EquipAction { get; set; }
    public Action<BaseUnit.EffectiveCharacteristics> InventoryAction { get; set; }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.AddToGroup(Groups.Loot);

        this.texture.Connect(CommonSignals.GuiInput, this, nameof(LootTextureClicked));
    }

    public void LootTextureClicked(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch ((ButtonList)mouseEvent.ButtonIndex)
            {
                case ButtonList.Left:
                    this.EmitSignal(nameof(LootClicked));
                    break;

                case ButtonList.Right:
                    this.buffDescriptionLabel.Text = LootDescription;
                    this.lootPopup.Show();
                    break;
            }
        }
    }
}

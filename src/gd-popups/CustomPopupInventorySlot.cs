using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomPopupInventorySlot.tscn")]
[Tool]
public partial class CustomPopupInventorySlot
{
    [Export]
    public int ItemIndex;
    private int itemsCount;

    [Export]
    public int ItemsCount
    {
        get => itemsCount; set
        {
            itemsCount = value;
            if (IsInsideTree())
            {
                this.countLabel.Text = value.ToString();
                this.countLabel.Visible = value > 1;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.RemoveItem();

        this.ItemsCount = itemsCount;
    }

    public bool HasItem()
    {
        return this.countLabel.Text != "0";
    }

    public void AddItem(Node loot, int item, int count)
    {
        if (HasItem())
        {
            GD.PrintErr("Can not add loot item as slot is already occupied.");
            return;
        }

        this.lootContainer.AddChild(loot);
        this.ItemIndex = item;
        this.ItemsCount = count;
    }

    public (int, int) GetItem()
    {
        if (this.HasItem())
        {
            return (this.ItemIndex, ItemsCount);
        }
        else
        {
            return (0, 0);
        }
    }

    public void UpdateCount(int countDiff)
    {
        int result;
        if (int.TryParse(this.countLabel.Text, out var current))
        {
            result = current + countDiff;
        }
        else
        {
            GD.PrintErr("Can not update count as original value is not set.");
            result = countDiff;
        }

        if (result < 0)
        {
            RemoveItem();
            return;
        }

        this.ItemsCount = result;
    }

    public void RemoveItem()
    {
        this.lootContainer.ClearChildren();
        this.ItemsCount = 0;
        this.ItemIndex = 0;
    }
}

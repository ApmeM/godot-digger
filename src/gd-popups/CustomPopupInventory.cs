using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomPopupInventory.tscn")]
[Tool]
public partial class CustomPopupInventory
{
    [Export]
    public PackedScene InventorySlot;

    [Export]
    public List<Texture> resources = new List<Texture>();

    private int size;
    [Export]
    public int Size
    {
        get
        {
            return this.size;
        }
        set
        {
            if (IsInsideTree() && this.slotContainer.GetChildCount() != value)
            {
                this.slotContainer.ClearChildren();
                for (var i = 0; i < value; i++)
                {
                    this.slotContainer.AddChild(this.InventorySlot.Instance());
                }
            }
            this.size = value;
        }
    }


    private int sizePerRow;
    [Export]
    public int SizePerRow
    {
        get => this.sizePerRow;
        set
        {
            if (IsInsideTree())
            {
                this.slotContainer.Columns = value;
            }
            this.sizePerRow = value;
        }
    }

    public bool TryAddItem(int itemIndex, int count)
    {GD.Print("TryAddResource");
        if (resources.Count <= itemIndex)
        {
            GD.PrintErr($"Resource with index {itemIndex} is not known for inventory.");
            return false;
        }

        var loot = new TextureRect
        {
            Texture = resources[itemIndex]
        };

        foreach (CustomPopupInventorySlot slot in this.slotContainer.GetChildren())
        {
            if (slot.HasItem())
            {
                continue;
            }

            slot.AddItem(loot, count);
            return true;
        }

        return false;
    }

    public void ClearItems()
    {
        foreach (CustomPopupInventorySlot slot in this.slotContainer.GetChildren())
        {
            if (!slot.HasItem())
            {
                continue;
            }

            slot.RemoveItem();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.SizePerRow = this.sizePerRow;
        this.Size = size;
    }
}

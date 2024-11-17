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
    public List<Texture> Resources = new List<Texture>();

    private uint size;

    [Export]
    public uint Size
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
                    var slot = this.InventorySlot.Instance<CustomPopupInventorySlot>();
                    this.slotContainer.AddChild(slot);
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
    {
        if (Resources.Count <= itemIndex)
        {
            GD.PrintErr($"Resource with index {itemIndex} is not known for inventory.");
            return false;
        }

        var loot = new TextureRect
        {
            Texture = Resources[itemIndex]
        };

        foreach (CustomPopupInventorySlot slot in this.slotContainer.GetChildren())
        {
            if (slot.HasItem())
            {
                continue;
            }

            slot.AddItem(loot, itemIndex, count);
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

    public IEnumerable<(int, int)> GetItems(){
        foreach (CustomPopupInventorySlot slot in this.slotContainer.GetChildren())
        {
            if (!slot.HasItem())
            {
                continue;
            }

            yield return slot.GetItem();
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

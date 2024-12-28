using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Inventory.tscn")]
[Tool]
public partial class Inventory
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
                    var slot = this.InventorySlot.Instance<InventorySlot>();
                    this.slotContainer.AddChild(slot);
                }
            }
            this.size = value;
        }
    }

    [Export]
    public int MaxCountPerSlot = 10;

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

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.SizePerRow = this.sizePerRow;
        this.Size = size;
    }

    public uint TryRemoveItems(int itemIndex, uint count)
    {
        if (count == 0)
        {
            return 0;
        }

        if (Resources.Count <= itemIndex)
        {
            GD.PrintErr($"Resource with index {itemIndex} is not known for inventory.");
            return count;
        }

        foreach (var slot in this.slotContainer.GetChildren().Cast<InventorySlot>().Reverse())
        {
            if (!slot.HasItem())
            {
                continue;
            }

            if (slot.GetItem().Item1 != itemIndex)
            {
                continue;
            }

            var currentCount = slot.GetItem().Item2;
            if (currentCount > count)
            {
                slot.UpdateCount(-(int)count);
                return 0;
            }

            slot.RemoveItem();
            count -= (uint)currentCount;
            
            if (count == 0)
            {
                return 0;
            }
        }

        return count;
    }

    public uint TryAddItem(int itemIndex, uint count)
    {
        if (count == 0)
        {
            return 0;
        }

        if (Resources.Count <= itemIndex)
        {
            GD.PrintErr($"Resource with index {itemIndex} is not known for inventory.");
            return count;
        }

        var loot = new TextureRect
        {
            Texture = Resources[itemIndex]
        };

        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            if (!slot.HasItem())
            {
                slot.SetItem(loot, itemIndex);
            }

            if (slot.GetItem().Item1 != itemIndex)
            {
                continue;
            }

            var currentCount = slot.GetItem().Item2;
            var spaceLeft = this.MaxCountPerSlot - currentCount;
            if (spaceLeft >= count)
            {
                slot.UpdateCount((int)count);
                return 0;
            }

            slot.UpdateCount(spaceLeft);
            count -= (uint)spaceLeft;
        }

        return count;
    }

    public void ClearItems()
    {
        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            if (!slot.HasItem())
            {
                continue;
            }

            slot.RemoveItem();
        }
    }

    public IEnumerable<(int, int)> GetItems()
    {
        return this.slotContainer.GetChildren()
            .OfType<InventorySlot>()
            .Where(a => a.HasItem())
            .Select(a => a.GetItem());
    }


    public int GetItemCount(int item)
    {
        return this.slotContainer.GetChildren()
            .OfType<InventorySlot>()
            .Where(a => a.HasItem())
            .Select(a => a.GetItem())
            .Where(a => a.Item1 == item)
            .Sum(a => a.Item2);
    }
}

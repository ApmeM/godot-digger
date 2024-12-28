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
                    slot.Resources = Resources;
                    slot.MaxCount = this.MaxCountPerSlot;
                    this.slotContainer.AddChild(slot);
                }
            }
            this.size = value;
        }
    }

    private int maxCountPerSlot;

    [Export]
    public int MaxCountPerSlot
    {
        get => maxCountPerSlot;
        set
        {
            maxCountPerSlot = value;
            if (IsInsideTree())
            {
                foreach (InventorySlot slot in this.slotContainer.GetChildren())
                {
                    slot.MaxCount = value;
                }
            }
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

    private string title;

    [Export]
    public string Title
    {
        get => this.title;
        set
        {
            if (IsInsideTree())
            {
                this.inventoreNameLabel.Text = value;
            }
            this.title = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.SizePerRow = this.sizePerRow;
        this.Size = size;
        this.Title = this.title;
        this.MaxCountPerSlot = this.maxCountPerSlot;
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

        var diff = -(int)count;
        foreach (var slot in this.slotContainer.GetChildren().Cast<InventorySlot>().Reverse())
        {
            diff = slot.TryAddItem(itemIndex, diff);
            if (diff == 0)
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

        var diff = (int)count;
        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            diff = slot.TryAddItem(itemIndex, diff);
            if (diff == 0)
            {
                return 0;
            }
        }

        return (uint)diff;
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

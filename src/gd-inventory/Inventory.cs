using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Inventory.tscn")]
[Tool]
public partial class Inventory
{
    [Export]
    public PackedScene InventorySlotScene;

    [Export]
    public List<Texture> Resources = new List<Texture>();

    [Export]
    public bool CanUseItem = true;

    [Signal]
    public delegate void UseItem(InventorySlot slot);

    [Signal]
    public delegate void DragAndDropComplete(InventorySlot slot);

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
                    var slot = this.InventorySlotScene.Instance<InventorySlot>();
                    slot.Resources = Resources;
                    slot.MaxCount = this.MaxCountPerSlot;
                    this.slotContainer.AddChild(slot);
                    slot.Connect(nameof(InventorySlot.UseItem), this, nameof(SlotUseItem), new Godot.Collections.Array { slot });
                    slot.Connect(nameof(InventorySlot.DragAndDropComplete), this, nameof(SlotDragAndDropComplete), new Godot.Collections.Array { slot });
                }
            }
            this.size = value;
        }
    }

    private void SlotUseItem(InventorySlot slot)
    {
        if (CanUseItem)
        {
            this.EmitSignal(nameof(UseItem), slot);
        }
    }

    private void SlotDragAndDropComplete(InventorySlot slot)
    {
        this.EmitSignal(nameof(DragAndDropComplete), slot);
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

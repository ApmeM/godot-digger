using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Inventory.tscn")]
[Tool]
public partial class Inventory
{
    public struct InventorySlotConfig
    {
        public Texture Texture;
        public int MaxCount;
        public Dictionary<int, int> MergeActions;
        public int ItemType;
    }

    public class InventoryConfig
    {
        public readonly Dictionary<int, InventorySlotConfig> SlotConfigs = new Dictionary<int, InventorySlotConfig>();
    }

    public InventoryConfig Config = new InventoryConfig();

    [Export]
    public PackedScene InventorySlotScene;

    [Export]
    public bool CanUseItem = true;

    [Signal]
    public delegate void UseItem(InventorySlot slot);

    [Signal]
    public delegate void DragAndDropComplete(InventorySlot from, InventorySlot to);

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
                    slot.Config = Config;
                    this.slotContainer.AddChild(slot);
                    slot.Connect(nameof(InventorySlot.UseItem), this, nameof(SlotUseItem), new Godot.Collections.Array { slot });
                    slot.Connect(nameof(InventorySlot.DragAndDropComplete), this, nameof(SlotDragAndDropComplete));
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

    private void SlotDragAndDropComplete(InventorySlot from, InventorySlot to)
    {
        this.EmitSignal(nameof(DragAndDropComplete), from, to);
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
    }

    public uint TryRemoveItem(int itemId, uint count)
    {
        if (count == 0)
        {
            return 0;
        }

        if (!Config.SlotConfigs.ContainsKey(itemId))
        {
            GD.PrintErr($"Resource with index {itemId} is not known for this inventory.");
            return count;
        }

        var diff = -(int)count;
        foreach (var slot in this.slotContainer.GetChildren().Cast<InventorySlot>().Reverse())
        {
            diff = slot.TryAddItem(itemId, diff);
            if (diff == 0)
            {
                return 0;
            }
        }

        return count;
    }

    public IEnumerable<(int, int)> TryRemoveItems(IEnumerable<(int, int)> items)
    {
        foreach (var item in items)
        {
            var result = TryRemoveItem(item.Item1, (uint)item.Item2);
            if (result != 0)
            {
                yield return (item.Item1, (int)result);
            }
        }
    }

    public uint TryAddItem(int itemId, uint count)
    {
        if (count == 0)
        {
            return 0;
        }

        if (!Config.SlotConfigs.ContainsKey(itemId))
        {
            GD.PrintErr($"Resource with index {itemId} is not known for this inventory.");
            return count;
        }

        var diff = (int)count;
        foreach (InventorySlot slot in this.slotContainer.GetChildren())
        {
            diff = slot.TryAddItem(itemId, diff);
            if (diff == 0)
            {
                return 0;
            }
        }

        return (uint)diff;
    }

    public IEnumerable<(int, int)> TryAddItems(IEnumerable<(int, int)> items)
    {
        foreach (var item in items)
        {
            var result = TryAddItem(item.Item1, (uint)item.Item2);
            if (result != 0)
            {
                yield return (item.Item1, (int)result);
            }
        }
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
        return this.GetItems()
            .Where(a => a.Item1 == item)
            .Sum(a => a.Item2);
    }
}

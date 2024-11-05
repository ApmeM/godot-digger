using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomPopupInventory.tscn")]
[Tool]
public partial class CustomPopupInventory
{
    [Export]
    public PackedScene InventorySlot;

    [Export]
    public int Size
    {
        get
        {
            return this.slotContainer.GetChildCount();
        }
        set
        {
            if (this.slotContainer.GetChildCount() == value)
            {
                return;
            }

            this.slotContainer.ClearChildren();
            for (var i = 0; i < value; i++)
            {
                this.slotContainer.AddChild(this.InventorySlot.Instance());
            }
        }
    }

    [Export]
    public int SizePerRow
    {
        get => this.slotContainer.Columns;
        set => this.slotContainer.Columns = value;
    }

    public bool TryAddItem(Node loot, int count)
    {
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
    }
}

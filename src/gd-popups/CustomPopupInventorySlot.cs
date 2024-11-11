using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomPopupInventorySlot.tscn")]
[Tool]
public partial class CustomPopupInventorySlot
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.RemoveItem();
    }

    public bool HasItem()
    {
        return this.countLabel.Text != "0";
    }

    public void AddItem(Node loot, int count)
    {
        if (HasItem())
        {
            GD.PrintErr("Can not add loot item as slot is already occupied.");
            return;
        }

        GD.Print($"Additem {loot}");
        this.lootContainer.AddChild(loot);
        this.countLabel.Text = count.ToString();
        this.countLabel.Visible = count > 1;
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

        this.countLabel.Text = result.ToString();
        this.countLabel.Visible = result > 1;
    }

    public void RemoveItem()
    {
        this.lootContainer.ClearChildren();
        this.countLabel.Visible = false;
        this.countLabel.Text = "0";
    }
}

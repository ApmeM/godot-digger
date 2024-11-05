using Godot;

[SceneReference("CustomPopupInventorySlot.tscn")]
[Tool]
public partial class CustomPopupInventorySlot
{
    private int count = 0;
    private Node loot = null;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public bool HasItem()
    {
        return this.count > 0;
    }

    public void AddItem(Node loot, int count)
    {
        GD.Print($"Additem {loot}");
        this.loot = loot;
        this.count = count;
        this.AddChild(loot);
    }

    public void RemoveItem()
    {
        this.RemoveChild(this.loot);
        this.count = 0;
    }
}

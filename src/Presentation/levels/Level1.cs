using Godot;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        foreach (var id in LootDefinition.LootByName)
        {
            this.stashInventory.Config.Add((int)id.Value.Id, new InventorySlot.InventorySlotConfig
            {
                Texture = id.Value.Image,
                MaxCount = id.Value.MaxCount * 10,
                ItemType = (int)id.Value.ItemType
            });
        }

        this.stash.Connect(nameof(BaseUnit.Clicked), this, nameof(StashClicked));
    }

    private async void StashClicked()
    {
        this.BagInventoryPopup.ConfigureInventory(this.stashInventory);

        this.stashInventory.Visible = true;
        this.BagInventoryPopup.Show();
        await this.BagInventoryPopup.ToMySignal(nameof(CustomPopup.PopupClosed));
        this.stashInventory.Visible = false;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        this.stashInventory.QueueFree();
    }
}

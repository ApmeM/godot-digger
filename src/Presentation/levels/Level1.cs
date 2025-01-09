using Godot;
using System.Linq;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.stashInventory.Connect(nameof(Inventory.UseItem), this, nameof(InventoryUseItem));
    }

    public override void InitMap(uint maxNumberOfTurns, uint inventorySlots, uint digPower)
    {
        base.InitMap(maxNumberOfTurns, inventorySlots, digPower);
        
        this.stashInventory.Resources = Resources;
    }

    public override void ShowPopup(Vector2 pos)
    {
        if (pos == new Vector2(4, 12))
        {
            signLabel.Text = "Welcome to our glorious city (Currently abandoned).\nTutorial: wood pile requires to click twice to clean.";
            signPopup.Show();
            return;
        }
        if (pos == new Vector2(0, 18))
        {
            signLabel.Text = "Tutorial: Click on the grass above to open the road.";
            signPopup.Show();
            return;
        }
        base.ShowPopup(pos);
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
        if (pos == new Vector2(16, 11))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level2");
            return;
        }
        if (pos == new Vector2(10, 3))
        {
            this.EmitSignal(nameof(ChangeLevel), "Woodcutter");
            return;
        }
        base.ChangeLevelClicked(pos);
    }

    public override void CustomConstructionClicked(Vector2 pos)
    {
        if (pos == new Vector2(1, 4))
        {
            this.stashInventoryPopup.Show();
            return;
        }
        base.CustomConstructionClicked(pos);
    }
}

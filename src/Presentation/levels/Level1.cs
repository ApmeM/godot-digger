using Godot;
using System.Linq;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void ShowPopup(Vector2 pos)
    {
        if (pos == new Vector2(4, 12))
        {
            signLabel.Text = "Welcome to our glorious city (Currently abandoned).\nTutorial: wood pile requires to click twice to clean.";
            customPopup.Show();
            return;
        }
        if (pos == new Vector2(0, 18))
        {
            signLabel.Text = "Tutorial: Click on the grass above to open the road.";
            customPopup.Show();
            return;
        }
        base.ShowPopup(pos);
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
        var resources = this.inventory.GetItems().Select(a => (Loot)a.Item1).ToList();
        if (pos == new Vector2(16, 11))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level2", resources);
            return;
        }
        if (pos == new Vector2(10, 3))
        {
            this.EmitSignal(nameof(ChangeLevel), "Woodcutter", resources);
            return;
        }
        base.ChangeLevelClicked(pos);
    }
}

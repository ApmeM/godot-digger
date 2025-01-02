using Godot;
using System;
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
        if (pos == new Vector2(4, 7))
        {
            signLabel.Text = "Welcome. Click on wood piles to clear them.";
            customPopup.Show();
            return;
        }

        if (pos == new Vector2(6, 7))
        {
            signLabel.Text = "Very good, see you on the next level.";
            customPopup.Show();
            return;
        }
        base.ShowPopup(pos);
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
        var resources = this.inventory.GetItems().Select(a => (Loot)a.Item1).ToList();
        this.EmitSignal(nameof(ChangeLevel), "Level2", resources);
        base.ChangeLevelClicked(pos);
    }
}

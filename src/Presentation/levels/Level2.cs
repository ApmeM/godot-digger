using Godot;
using System.Linq;

[SceneReference("Level2.tscn")]
public partial class Level2
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
        var resources = this.inventory.GetItems().Select(a => (Loot)a.Item1).ToList();
        if (pos == new Vector2(8, 15))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level1", resources);
            return;
        }
        if (pos == new Vector2(6, 1))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level3", resources);
            return;
        }
        base.ChangeLevelClicked(pos);
    }
}

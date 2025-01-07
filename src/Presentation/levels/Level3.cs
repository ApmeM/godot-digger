using Godot;
using System.Linq;

[SceneReference("Level3.tscn")]
public partial class Level3
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void ChangeLevelClicked(Vector2 pos)
    {
        var resources = this.inventory.GetItems().Select(a => (Loot)a.Item1).ToList();
        if (pos == new Vector2(2, 2))
        {
            this.EmitSignal(nameof(ChangeLevel), "Level2", resources);
            return;
        }
        base.ChangeLevelClicked(pos);
    }
}

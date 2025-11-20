using Godot;

[SceneReference("PotionBrown.tscn")]
public partial class PotionBrown
{
    public PotionBrown()
    {
        this.UseAction = async (game) =>
        {
            game.CurrentLevel.HeaderControl.TrackingUnit.Inventory.Hide();
            var pos = await game.CurrentLevel.ChoosePosition();
            if (pos != null)
            {
                game.CurrentLevel.AddUnit(pos.Value, nameof(Tree));
            }
            game.CurrentLevel.HeaderControl.TrackingUnit.Inventory.Show();
            return pos != null;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

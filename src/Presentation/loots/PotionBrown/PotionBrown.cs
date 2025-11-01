using Godot;

[SceneReference("PotionBrown.tscn")]
public partial class PotionBrown
{
    public PotionBrown()
    {
        this.Price = 1;
        this.MaxCount = 1;
        this.UseAction = async (game) =>
        {
            game.HeaderControl.BagInventoryPopup.Hide();
            var pos = await game.CurrentLevel.ChoosePosition();
            if (pos != null)
            {
                game.CurrentLevel.AddUnit(pos.Value, nameof(Tree));
            }
            game.HeaderControl.BagInventoryPopup.Show();
            return pos != null;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

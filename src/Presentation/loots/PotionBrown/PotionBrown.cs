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
            var pos = await game.CurrentLevel.ChoosePosition();
            if (pos != null)
            {
                game.CurrentLevel.AddUnit(pos.Value, nameof(Tree));
            }
            return pos != null;
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

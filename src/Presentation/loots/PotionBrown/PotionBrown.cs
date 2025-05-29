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
            var pos = await this.ToSignal(game.CurrentLevel, nameof(BaseLevel.CellClicked));
            game.CurrentLevel.AddUnit((Vector2)pos[0], nameof(Tree));
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

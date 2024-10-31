using Godot;

[SceneReference("Stamina.tscn")]
public partial class Stamina
{
    private GameState gameState;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.gameState = this.GetNode<GameState>("/root/Main/GameState");
        this.gameState.Connect(nameof(GameState.NumberOfTurnsChanged), this, nameof(NumberOfTurnsChanged));

    }

    private void NumberOfTurnsChanged()
    {
        this.turnsCount.Text = this.gameState.NumberOfTurns.ToString();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        this.staminaProgress.Value = this.gameState.NumberOfTurnsCurrentRecoveryTime * 100 / this.gameState.NumberOfTurnsRecoveryTime;
    }
}

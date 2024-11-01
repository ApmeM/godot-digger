using System;
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
        if (this.gameState.NumberOfTurns == this.gameState.NumberOfTurnsMax)
        {
            this.staminaProgress.Value = 0;
        }
        else
        {
            this.staminaProgress.Value = (DateTime.Now - this.gameState.NumberOfTurnsLastUpdate).TotalSeconds * 100 / this.gameState.NumberOfTurnsRecoverySeconds;
        }
    }
}

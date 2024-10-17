using Godot;

[SceneReference("GameState.tscn")]
public partial class GameState
{
    private int numberOfTurns;

    [Signal]
    public delegate void NumberOfTurnsChanged();

    public int NumberOfTurns
    {
        get => numberOfTurns;
        set
        {
            numberOfTurns = value;
            this.EmitSignal(nameof(NumberOfTurnsChanged));
        }
    }

    public override void _Ready()
    {

    }
}

using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomPopup.tscn")]
[Tool]
public partial class CustomPopup
{
    [Signal]
    public delegate void PopupClosed();

    private string title;

    [Export]
    public string Title
    {
        get => title;
        set
        {
            if (IsInsideTree())
            {
                this.titleLabel.Text = value;
            }
            title = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.closeButton.Connect(CommonSignals.Pressed, this, nameof(BackButtonPressed));
        Title = title;
    }

    private void BackButtonPressed()
    {
        this.Hide();
        this.EmitSignal(nameof(PopupClosed));
    }
}

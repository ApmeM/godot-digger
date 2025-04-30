using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Sign.tscn")]
[Tool]
public partial class Sign
{
    private string text;

    [Export(PropertyHint.MultilineText)]
    public string Text
    {
        get => text;
        set
        {
            text = value;
            if (IsInsideTree())
            {
                this.signLabel.Text = text;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Connect(CommonSignals.Pressed, this, nameof(SignClicked));
        this.Text = text;
    }

    private void SignClicked()
    {
        signPopup.Show();
    }
}

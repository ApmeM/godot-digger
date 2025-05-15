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

        this.Text = text;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouse && mouse.IsPressed() && !mouse.IsEcho() && (ButtonList)mouse.ButtonIndex == ButtonList.Left)
        {
            var size = animatedSprite.Frames.GetFrame(animatedSprite.Animation, animatedSprite.Frame).GetSize();
            var rect = new Rect2(this.animatedSprite.Position, size);
            var mousePos = this.GetLocalMousePosition();
            
            if (rect.HasPoint(mousePos))
            {
                this.GetTree().SetInputAsHandled();
                this.SignClicked();
            }
        }
    }

    private void SignClicked()
    {
        signPopup.Show();
    }
}

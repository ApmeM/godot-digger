using Godot;

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

        Title = title;
#if DEBUG
        this.GetTree().EditedSceneRoot?.SetEditableInstance(this, true);
        this.SetDisplayFolded(true);
#endif
    }

    protected virtual void Close()
    {
        this.Hide();
        this.EmitSignal(nameof(PopupClosed));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (!Visible)
        {
            return;
        }

        if (@event is InputEventMouse mouse && ((ButtonList)mouse.ButtonMask & ButtonList.Left) == ButtonList.Left)
        {
            Close();
        }
        if (@event is InputEventKey key && ((KeyList)key.Scancode & KeyList.Escape) == KeyList.Escape)
        {
            Close();
        }
    }
}

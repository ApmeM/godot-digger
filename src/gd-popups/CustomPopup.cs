using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomPopup.tscn")]
[Tool]
public partial class CustomPopup
{
    [Signal]
    public delegate void PopupClosed();

    [Export]
    public bool CloseOnClickOutside = true;

    [Export]
    public bool CloseOnClickButton
    {
        get => closeOnClickButton;
        set
        {
            closeOnClickButton = value;
            if (IsInsideTree())
            {
                this.closeButtonContainer.Visible = value;
            }
        }
    }

    private string title;
    private bool closeOnClickButton;

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

        this.Title = this.title;
        this.CloseOnClickButton = this.closeOnClickButton;

        this.closeButton.Connect(CommonSignals.Pressed, this, nameof(Close));

#if DEBUG
        this.GetTree().EditedSceneRoot?.SetEditableInstance(this, true);
        this.SetDisplayFolded(true);
#endif
    }

    public virtual void Close()
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

        if (@event is InputEventMouse mouse && ((ButtonList)mouse.ButtonMask & ButtonList.Left) == ButtonList.Left && CloseOnClickOutside)
        {
            Close();
        }

        if (@event is InputEventKey key && ((KeyList)key.Scancode & KeyList.Escape) == KeyList.Escape)
        {
            Close();
        }
    }
}

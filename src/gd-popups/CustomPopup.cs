using Godot;

[SceneReference("CustomPopup.tscn")]
[Tool]
public partial class CustomPopup
{
    [Signal]
    public delegate void PopupClosed();

    private bool closeOnClickOutside = true;

    [Export]
    public bool CloseOnClickOutside
    {
        get => closeOnClickOutside;
        set
        {
            closeOnClickOutside = value;
            if (this.IsInsideTree())
            {
                this.outsidePopupButton.Disabled = !value;
            }
        }
    }

    private bool closeOnClickButton;

    [Export]
    public bool CloseOnClickButton
    {
        get => closeOnClickButton;
        set
        {
            closeOnClickButton = value;
            if (IsInsideTree())
            {
                this.closeButton.Visible = value;
            }
        }
    }

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

        this.Title = this.title;
        this.CloseOnClickButton = this.closeOnClickButton;
        this.CloseOnClickOutside = this.closeOnClickOutside;

        this.closeButton.Connect(CommonSignals.Pressed, this, nameof(Close));
        this.outsidePopupButton.Connect(CommonSignals.Pressed, this, nameof(Close));

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
}

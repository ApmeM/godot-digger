using Godot;

[SceneReference("CustomPopup.tscn")]
[Tool]
public partial class CustomPopup
{
    [Signal]
    public delegate void PopupClosed();

    [Export]
    public bool CloseOnClickOutside { get; set; } = true;

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

        this.closeButton.Connect(CommonSignals.Pressed, this, nameof(CloseButton));
        this.outsidePopupButton.Connect(CommonSignals.Pressed, this, nameof(CloseOutside));

#if DEBUG
        this.GetTree().EditedSceneRoot?.SetEditableInstance(this, true);
        this.SetDisplayFolded(true);
#endif
    }

    private void CloseButton()
    {
        if (this.CloseOnClickButton)
        {
            this.Close();
        }
    }
    private void CloseOutside()
    {
        if (this.CloseOnClickOutside)
        {
            this.Close();
        }
    }

    public virtual void Close()
    {
        this.Hide();
        this.EmitSignal(nameof(PopupClosed));
    }
}

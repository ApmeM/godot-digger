using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("CustomConfirmPopup.tscn")]
[Tool]
public partial class CustomConfirmPopup
{
    [Signal]
    public delegate void YesClicked();

    [Signal]
    public delegate void NoClicked();

    [Signal]
    public delegate void ChoiceMade(bool isYes);

    private bool allowYes;

    [Export]
    public bool AllowYes
    {
        get => allowYes;
        set
        {
            allowYes = value;
            if (IsInsideTree())
            {
                this.buttonYes.Disabled = !value;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.buttonYes.Connect(CommonSignals.Pressed, this, nameof(YesButtonClicked));
        this.buttonNo.Connect(CommonSignals.Pressed, this, nameof(NoButtonClicked));

        this.ShowCloseButton = false;
        this.AllowYes = this.allowYes;
    }

    protected override void Close()
    {
        this.EmitSignal(nameof(NoClicked));
        this.EmitSignal(nameof(ChoiceMade), false);
        base.Close();
    }

    private void NoButtonClicked()
    {
        this.EmitSignal(nameof(NoClicked));
        this.EmitSignal(nameof(ChoiceMade), false);
        base.Close();
    }

    private void YesButtonClicked()
    {
        this.EmitSignal(nameof(YesClicked));
        this.EmitSignal(nameof(ChoiceMade), true);
        base.Close();
    }
}

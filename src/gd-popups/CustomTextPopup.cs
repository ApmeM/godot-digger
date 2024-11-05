using Godot;

[SceneReference("CustomTextPopup.tscn")]
[Tool]
public partial class CustomTextPopup
{
    [Export(PropertyHint.MultilineText)]
    public string ContentText
    {
        get => this.contentText.Text;
        set => this.contentText.Text = value;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

using Godot;
using System;
using System.Threading.Tasks;

[SceneReference("FloatingTextManager.tscn")]
[Tool]
public partial class FloatingTextManager
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    [Export]
    public Vector2 Direction = new Vector2(0, -80);

    [Export]
    public float Duration = 2;

    [Export]
    public float Spread = (float)Math.PI / 2;

    [Export]
    public bool Highlite = true;

    public Label defaultControl => this.defaultLabel;

    public async Task ShowValueAsync(Control node, Vector2 at)
    {
        var t = (Tween)this.tween.Duplicate();
        AddChild(node);
        AddChild(t);

        var movement = Direction.Rotated((float)GD.RandRange(-Spread / 2, Spread / 2));

        node.RectPosition = at;
        node.RectPivotOffset = node.RectSize / 2;

        t.InterpolateProperty(node, "rect_position", node.RectPosition, node.RectPosition + movement, Duration, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        t.InterpolateProperty(node, "modulate:a", 1.0, 0.0, Duration, Tween.TransitionType.Linear, Tween.EaseType.InOut);

        if (Highlite)
        {
            t.InterpolateProperty(node, "rect_scale", node.RectScale * 2, node.RectScale, 0.4f, Tween.TransitionType.Back, Tween.EaseType.In);
        }

        t.Start();
        await ToSignal(t, "tween_all_completed");

        node.QueueFree();
        t.QueueFree();
        RemoveChild(node);
        RemoveChild(t);
    }

    public async Task ShowValueAsync(PackedScene scene, Vector2 at)
    {
        var control = scene.Instance<Control>();
        await this.ShowValueAsync(control, at);
    }

    public async Task ShowValueAsync(string value, Vector2 at, Color? color = null)
    {
        var label = (Label)defaultLabel.Duplicate();
        label.Text = value;
        label.Modulate = color ?? new Color(1, 1, 1);

        await this.ShowValueAsync(label, at);
    }

    public async void ShowValue(Control node, Vector2 at) =>
        await this.ShowValueAsync(node, at);

    public async void ShowValue(PackedScene scene, Vector2 at) =>
        await this.ShowValueAsync(scene, at);

    public async void ShowValue(string value, Vector2 at, Color? color = null) =>
        await this.ShowValueAsync(value, at, color);
}

using Godot;
using System;

[SceneReference("DraggableCamera.tscn")]
public partial class DraggableCamera
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    private bool drag = false;
    private Vector2 initPosMouse = Vector2.Zero;

    [Export]
    public float minimumZoom = 0.3f;

    [Export]
    public float maximumZoom = 3f;

    [Export]
    public bool enabled = true;
    /// <summary>
    ///     The zoom value should be between -1 and 1. 
    ///     This value is then translated to be from minimumZoom to maximumZoom.
    ///     This lets you set appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
    /// </summary>
    public float NormalizedZoom
    {
        get
        {
            if (this.Zoom.x == 1)
                return 0f;

            if (this.Zoom.x < 1)
                return Map(this.Zoom.x, this.minimumZoom, 1, -1, 0);
            return Map(this.Zoom.x, 1, this.maximumZoom, 0, 1);
        }
        set
        {
            var newZoom = Mathf.Clamp(value, -1, 1);
            if (newZoom == 0)
                this.Zoom = Vector2.One;
            else if (newZoom < 0)
                this.Zoom = Vector2.One * Map(newZoom, -1, 0, this.minimumZoom, 1);
            else
                this.Zoom = Vector2.One * Map(newZoom, 0, 1, 1, this.maximumZoom);
        }
    }

    /// <summary>
    ///     Minimum non-scaled value (0 - float.Max) that the camera zoom can be. 
    ///     Defaults to 0.3
    /// </summary>
    public float MinimumZoom
    {
        get => this.minimumZoom;
        set
        {
            if (value <= 0)
            {
                throw new Exception("MinimumZoom must be greater then zero.");
            }

            if (this.Zoom.x < value)
                this.Zoom = Vector2.One * value;

            this.minimumZoom = value;
        }
    }

    /// <summary>
    ///     maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
    /// </summary>
    /// <value>The maximum zoom.</value>
    public float MaximumZoom
    {
        get => this.maximumZoom;
        set
        {
            if (value <= 0)
            {
                throw new Exception("MaximumZoom must be greater then zero.");
            }

            if (this.Zoom.x > value)
                this.Zoom = Vector2.One * value;

            this.maximumZoom = value;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (!enabled)
        {
            return;
        }

        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (((ButtonList)mouseMotion.ButtonMask & ButtonList.MaskLeft) == ButtonList.Left)
            {
                if (drag)
                {
                    this.GlobalPosition = (this.GetViewport().Size / 2 - this.GetViewport().CanvasTransform.origin) / this.GetViewport().CanvasTransform.Scale;
                    var mousePos = this.GetViewport().GetMousePosition();
                    this.GlobalPosition += (initPosMouse - mousePos) * this.Zoom;
                    this.initPosMouse = mousePos;
                }
                else
                {
                    if (initPosMouse == Vector2.Zero)
                    {
                        this.initPosMouse = this.GetViewport().GetMousePosition();
                    }

                    this.drag = this.initPosMouse != this.GetViewport().GetMousePosition();
                }
            }
            else
            {
                this.drag = false;
                this.initPosMouse = Vector2.Zero;
            }
        }

        if (@event is InputEventMouseButton buttonEvent)
        {
            if (buttonEvent.ButtonIndex == (int)ButtonList.WheelUp)
            {
                this.NormalizedZoom += 0.1f;
            }
            if (buttonEvent.ButtonIndex == (int)ButtonList.WheelDown)
            {
                this.NormalizedZoom -= 0.1f;
            }
        }
    }

    private static float Map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
    {
        return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
    }
}

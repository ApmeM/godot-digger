using System;
using System.Collections.Specialized;
using Godot;

[SceneReference("TypewriterLabel.tscn")]
public partial class TypewriterLabel
{
    [Export]
    public float Speed;

    [Signal]
    public delegate void TypingFinished();

    private float currentDelay = 0;

    public bool IsTyping = false;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && ((ButtonList)mouseEvent.ButtonMask).HasFlag(ButtonList.Left))
        {
            if (ForceFinish())
            {
                this.GetTree().SetInputAsHandled();
            }
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && (((KeyList)keyEvent.Scancode).HasFlag(KeyList.Space) || ((KeyList)keyEvent.Scancode).HasFlag(KeyList.Escape)))
        {
            if (ForceFinish())
            {
                this.GetTree().SetInputAsHandled();
            }
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (!this.IsTyping)
        {
            return;
        }
        
        this.currentDelay += delta;
        if (this.currentDelay <= this.Speed)
        {
            return;
        }

        this.currentDelay -= this.Speed;

        this.VisibleCharacters++;
        if (this.VisibleCharacters >= this.Text.Length)
        {
            this.EmitSignal(nameof(TypingFinished));
            this.Pause();
        }
    }

    public void Start()
    {
        this.VisibleCharacters = 0;
        this.currentDelay = 0;
        this.Resume();
    }

    public void Pause()
    {
        this.IsTyping = false;
    }

    public void Resume()
    {
        this.IsTyping = true;        
    }

    public bool ForceFinish()
    {
        if (!this.IsTyping)
        {
            return false;
        }

        this.EmitSignal(nameof(TypingFinished));
        this.VisibleCharacters = this.Text.Length;
        this.Pause();
        return true;
    }
}

using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Wardrobe.tscn")]
public partial class Wardrobe
{
    public Wardrobe()
    {
        this.HP = 4;
        this.Loot = new List<string> { nameof(Cloth) };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
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
                this.UnitClicked();
            }
        }
    }

}

using System;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Grandma.tscn")]
public partial class Grandma
{
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
                this.GrandmaClicked();
            }
        }
    }

    private async void GrandmaClicked()
    {
        this.questPopup.BagInventoryPath = level.BagInventoryPopup.GetPath();

        var result = await questPopup.ShowQuestPopup("Did you bring me a bread from my grand daughter RedHat?",
            new[] { (nameof(Bread), 1u) },
            new[] { (nameof(Gold), 1u) }
        );

        if (result)
        {
            this.signPopup.Show();
        }
    }
}

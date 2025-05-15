using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Redhat.tscn")]
public partial class Redhat
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
                this.RedHatClicked();
            }
        }
    }

    private async void RedHatClicked()
    {
        this.questPopup.BagInventoryPath = level.BagInventoryPopup.GetPath();

        var result = await questPopup.ShowQuestPopup("Hi strong man, I afraid to go to my grandma through the forrest. There are a lot of wolfs. For each wolf skin I'll gibe you a bread that is very tasty (doubleclick on it from the inventory).",
            new[] { (nameof(WolfSkin), 1u) },
            new[] { (nameof(Bread), 1u) }
        );

        if (result)
        {
            this.signPopup.Show();
        }
    }
}

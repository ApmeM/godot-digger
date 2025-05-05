using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Redhat.tscn")]
public partial class Redhat
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(GrandmaClicked));
    }

    private async void GrandmaClicked()
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

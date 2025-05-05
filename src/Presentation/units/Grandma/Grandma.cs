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

        this.texture.Connect(CommonSignals.Pressed, this, nameof(GrandmaClicked));
    }

    private async void GrandmaClicked()
    {
        var level = this.GetNode<BaseLevel>(this.LevelPath);
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

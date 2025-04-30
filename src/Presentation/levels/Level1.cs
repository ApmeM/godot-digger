using System.Collections.Generic;
using Godot;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }


    // public override async void CustomBlockClicked(Vector2 pos)
    // {
    //  ToDo: Quests
    //     if (pos == new Vector2(24, 17))
    //     {
    //         var result = await questPopup.ShowQuestPopup("Did you bring me a bread from my grand daughter RedHat?",
    //             new[] { (Loot.Bread, 1u) },
    //             new[] { (Loot.Gold, 1u) }
    //         );
    //         if (result)
    //         {
    //             // this.ShowPopup("Thank you young man.");
    //         }
    //         return;
    //     }
    //     if (pos == new Vector2(9, 13))
    //     {
    //         var result = await questPopup.ShowQuestPopup("Hi strong man, I afraid to go to my grandma through the forrest. There are a lot of wolfs. For each wolf skin I'll gibe you a bread that is very tasty (doubleclick on it from the inventory).",
    //             new[] { (Loot.WolfSkin, 1u) },
    //             new[] { (Loot.Bread, 1u) }
    //         );
    //         if (result)
    //         {
    //             // this.ShowPopup("Thanks. Take your bread.");
    //         }
    //         return;
    //     }
    //     base.CustomBlockClicked(pos);
    // }

}

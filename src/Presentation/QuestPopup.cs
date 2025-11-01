using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

[SceneReference("QuestPopup.tscn")]
public partial class QuestPopup
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public Task<bool> ShowQuestPopup(string description, BagInventoryData inventory, List<QuestData> requirements, List<QuestData> rewards)
    {
        this.Content = description;
        return ShowQuestPopup(inventory, requirements, rewards);
    }

    public async Task<bool> ShowQuestPopup(BagInventoryData inventory, List<QuestData> requirements, List<QuestData> rewards)
    {
        this.requirementsList.RemoveChildren();

        var isEnough = true;
        foreach (var req in requirements)
        {
            var definition = LootDefinition.LootByName[req.Loot.GetState().GetNodeName(0)];
            this.requirementsList.AddChild(new TextureRect
            {
                Texture = definition.Image
            });

            var existing = inventory.GetItemCount(definition.Name);

            this.requirementsList.AddChild(new Label
            {
                Text = $"x {existing} / {req.Count}"
            });

            isEnough = isEnough && existing >= req.Count;
        }

        this.AllowYes = isEnough;
        this.Show();
        var decision = await this.ToMySignal<bool>(nameof(CustomConfirmPopup.ChoiceMade));

        if (!isEnough)
        {
            return false;
        }

        if (!decision)
        {
            return false;
        }

        var success = inventory.TryChangeCountsOrCancel(
            requirements
                .Select(a => (a.Loot.GetState().GetNodeName(0), -(int)a.Count))
                .Concat(rewards.Select(a => (a.Loot.GetState().GetNodeName(0), (int)a.Count))));

        return success;
    }
}

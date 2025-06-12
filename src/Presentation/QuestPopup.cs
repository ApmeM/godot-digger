using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("QuestPopup.tscn")]
public partial class QuestPopup
{
    [Export]
    public NodePath BagInventoryPath;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public Task<bool> ShowQuestPopup(string description, List<QuestData> requirements, List<QuestData> rewards)
    {
        this.Content = description;
        return ShowQuestPopup(requirements, rewards);
    }

    public async Task<bool> ShowQuestPopup(List<QuestData> requirements, List<QuestData> rewards)
    {
        var inventory = this.GetNode<BagInventoryPopup>(this.BagInventoryPath);

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
        var decision = (bool)(await ToSignal(this, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);

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

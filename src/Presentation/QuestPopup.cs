using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("QuestPopup.tscn")]
public partial class QuestPopup
{
    [Export]
    public NodePath BagInventoryPath;

    [Export]
    public TileSet LootTileSet;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public async Task<bool> ShowQuestPopup(string description, ValueTuple<ValueTuple<int, int, int>, uint>[] requirements, ValueTuple<ValueTuple<int, int, int>, uint>[] rewards)
    {
        var inventory = this.GetNode<BagInventoryPopup>(this.BagInventoryPath);

        this.Content = description;
        this.requirementsList.RemoveChildren();

        var isEnough = true;
        foreach (var req in requirements)
        {
            var lootId = req.Item1.Item1;

            this.requirementsList.AddChild(new TextureRect
            {
                Texture = this.LootTileSet.TileGetTexture(lootId)
            });

            var existing = inventory.GetItemCount(lootId);

            this.requirementsList.AddChild(new Label
            {
                Text = $"x {existing} / {req.Item2}"
            });

            isEnough = isEnough && existing >= req.Item2;
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
                .Select(a => (a.Item1.Item1, -(int)a.Item2))
                .Concat(rewards.Select(a => (a.Item1.Item1, (int)a.Item2))));

        return success;
    }

}

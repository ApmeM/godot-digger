using System;
using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Building.tscn")]
public partial class Building
{
    [Export]
    public NodePath InventoryPath;

    private Func<List<Tuple<Loot, uint>>> getRequirements;

    private Action action;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Connect(CommonSignals.Pressed, this, nameof(BuildingPressed));
    }

    public void Initialize(Func<List<Tuple<Loot, uint>>> getRequirements, Action action)
    {
        this.getRequirements = getRequirements;
        this.action = action;
    }

    private async void BuildingPressed()
    {
        var requirements = this.getRequirements();
        var inventory = this.GetNode<Inventory>(InventoryPath);

        this.requirementsList.ClearChildren();

        var isEnough = true;
        foreach (var req in requirements)
        {
            this.requirementsList.AddChild(new TextureRect
            {
                Texture = inventory.Resources[(int)req.Item1]
            });

            var existing = inventory.GetItemCount((int)req.Item1);

            this.requirementsList.AddChild(new Label
            {
                Text = $"x {existing} / {req.Item2}"
            });


            isEnough = isEnough && existing >= req.Item2;
        }

        this.confirmation.AllowYes = isEnough;
        this.confirmation.ShowCentered();

        if (!isEnough)
        {
            return;
        }

        this.confirmation.ShowCentered();
        var decision = (bool)(await ToSignal(this.confirmation, nameof(CustomConfirmPopup.ChoiceMade))).GetValue(0);
        if (decision)
        {
            foreach (var req in requirements)
            {
                var existing = inventory.GetItemCount((int)req.Item1);
                if (existing >= req.Item2)
                {
                    inventory.TryRemoveItems((int)req.Item1, req.Item2);
                    action();
                }
            }
        }
    }
}

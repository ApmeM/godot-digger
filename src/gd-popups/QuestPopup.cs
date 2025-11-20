using System.Collections.Generic;
using System.Linq;
using Godot;

[SceneReference("QuestPopup.tscn")]
public partial class QuestPopup
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.buttonYes.Connect(CommonSignals.Pressed, this, nameof(YesClicked));
        this.buttonNo.Connect(CommonSignals.Pressed, this, nameof(NoClicked));
        this.buttonNext.Connect(CommonSignals.Pressed, this, nameof(NextClicked));
        this.contentTextLabel.Connect(nameof(TypewriterLabel.TypingFinished), this, nameof(TypingFinished));
    }

    public Queue<QuestPopupData> PopupData;
    private BagInventoryPopup inventory;

    private void TypingFinished()
    {
        if (this.PopupData.Peek().Requirements != null)
        {
            ShowQuest();
        }
    }

    private void NextClicked()
    {
        if (this.contentTextLabel.IsTyping)
        {
            this.contentTextLabel.ForceFinish();
            if (this.PopupData.Peek().Requirements != null)
            {
                ShowQuest();
            }
        }
        else
        {
            this.PopupData.Dequeue();
            this.Rebuild();
        }
    }

    private void NoClicked()
    {
        this.Hide();
    }

    private void YesClicked()
    {
        var dialogData = this.PopupData.Peek();
        var success = inventory.TryChangeCountsOrCancel(
            dialogData.Requirements
                .Select(a => (a.LootName, -(int)a.Count))
                .Concat(dialogData.Rewards.Select(a => (a.LootName, (int)a.Count))));

        if (success)
        {
            this.PopupData.Dequeue();
            this.Rebuild();
        }
    }

    private void Rebuild()
    {
        if (this.PopupData.Count == 0)
        {
            Hide();
            return;
        }

        this.requirementsList.RemoveChildren();

        var dialogData = this.PopupData.Peek();
        this.contentTextLabel.Text = dialogData.Description;
        this.contentTextLabel.Start();
        this.buttonNext.Visible = true;
        this.buttonYes.Visible = false;
        this.buttonNo.Visible = false;
    }

    private void ShowQuest()
    {
        this.buttonNext.Visible = false;
        this.buttonYes.Visible = true;
        this.buttonNo.Visible = true;

        var dialogData = this.PopupData.Peek();

        var isEnough = true;
        foreach (var req in dialogData.Requirements)
        {
            var definition = LootDefinition.LootByName[req.LootName];
            this.requirementsList.AddChild(new TextureRect
            {
                Texture = definition.Image
            });

            var existing = this.inventory.GetItemCount(definition.Name);

            this.requirementsList.AddChild(new Label
            {
                Text = $"x {existing} / {req.Count}"
            });

            isEnough = isEnough && existing >= req.Count;
        }

        this.buttonYes.Disabled = !isEnough;
    }

    public void ShowQuestPopup(BagInventoryPopup inventory)
    {
        this.inventory = inventory;
        this.Rebuild();
        this.Show();
    }
}

using System;
using System.Collections.Generic;
using BrainAI.AI.UtilityAI;
using Godot;
using MonoCustomResourceRegistry;

[Serializable]
[Tool]
[RegisteredType(nameof(CustomActionTalk))]
public class CustomActionTalk : Resource, IIntent<BaseUnit>
{
    [Export]
    public List<QuestPopupData> QuestData;

    [Export]
    public NodePath QuestPopupPath;

    [Export]
    public NodePath InventoryUnitPath;

    public void Enter(BaseUnit context)
    {
     
        var questPopup = context.GetNode<QuestPopup>(this.QuestPopupPath);
        questPopup.PopupData = new Queue<QuestPopupData>(QuestData);
        if (this.InventoryUnitPath != null)
        {
            var unit = context.GetNode<BaseUnit>(this.InventoryUnitPath);
            questPopup.ShowQuestPopup(unit.Inventory);
        }
        else
        {
            questPopup.ShowQuestPopup(null);
        }
    }

    public bool Execute(BaseUnit context)
    {
        var questPopup = context.GetNode<QuestPopup>(this.QuestPopupPath);
        return !questPopup.Visible;
    }

    public void Exit(BaseUnit context)
    {
    }
}

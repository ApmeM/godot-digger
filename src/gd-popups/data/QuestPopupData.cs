using System.Collections.Generic;
using Godot;
using System;
using MonoCustomResourceRegistry;


[Serializable]
[Tool]
[RegisteredType(nameof(QuestPopupData))]
public class QuestPopupData : Resource
{
    [Export]
    public string Description;

    [Export]
    public List<QuestData> Requirements;

    [Export]
    public List<QuestData> Rewards;
}


using System;
using Godot;
using MonoCustomResourceRegistry;

[Serializable]
[Tool]
[RegisteredType(nameof(QuestData))]
public class QuestData : Resource
{
    [Export]
    public PackedScene Loot;

    public string LootName => Loot.GetState().GetNodeName(0);

    [Export]
    public uint Count;
}

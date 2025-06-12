
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

    [Export]
    public uint Count;
}
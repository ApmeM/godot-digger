using System;
using System.Collections.Generic;
using System.Linq;
using BrainAI.AI.UtilityAI;
using Godot;
using MonoCustomResourceRegistry;

[Serializable]
[Tool]
[RegisteredType(nameof(CustomActionAttackOpponent))]
public class CustomActionAllAtOnce : Resource, IIntent<BaseUnit>
{
    [Export]
    public List<Resource> CustomActions;

    public void Enter(BaseUnit context)
    {
        CustomActions.ForEach(delegate (Resource a)
        {
            ((IIntent<BaseUnit>)a).Enter(context);
        });
    }

    public bool Execute(BaseUnit context)
    {
        CustomActions.RemoveAll((Resource a) => ((IIntent<BaseUnit>)a).Execute(context));
        return !CustomActions.Any();
    }

    public void Exit(BaseUnit context)
    {
        CustomActions.ForEach(delegate (Resource a)
        {
            ((IIntent<BaseUnit>)a).Exit(context);
        });
    }
}
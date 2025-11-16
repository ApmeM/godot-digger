using System;
using System.Collections.Generic;
using BrainAI.AI.UtilityAI;
using Godot;
using MonoCustomResourceRegistry;

[Serializable]
[Tool]
[RegisteredType(nameof(CustomActionQueue))]
public class CustomActionQueue : Resource, IIntent<BaseUnit>
{
    // Resource should be of type IIntent<BaseUnit>
    [Export]
    public List<Resource> CustomActions;

    // Resource should be of type IIntent<BaseUnit>
    [Export]
    public Resource CurrentAction;

    public void Enter(BaseUnit context)
    {
        ((IIntent<BaseUnit>)CurrentAction)?.Enter(context);
    }

    public bool Execute(BaseUnit context)
    {
        if (CurrentAction == null)
        {
            if (CustomActions.Count == 0)
            {
                return true;
            }

            CurrentAction = CustomActions[0];
            ((IIntent<BaseUnit>)CurrentAction).Enter(context);
        }

        if (((IIntent<BaseUnit>)CurrentAction).Execute(context))
        {
            ((IIntent<BaseUnit>)CurrentAction).Exit(context);
            CustomActions.Remove(CurrentAction);
            CurrentAction = null;
        }

        return false;
    }

    public void Exit(BaseUnit context)
    {
        ((IIntent<BaseUnit>)CurrentAction)?.Exit(context);
    }
}

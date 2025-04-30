using System.Collections.Generic;
using Godot;

[SceneReference("PotionEmpty.tscn")]
public partial class PotionEmpty
{
    public PotionEmpty()
    {
        Price = 1;
        MaxCount = 1;
        MergeActions = new Dictionary<string, string>{
            {nameof(StaminaPlant), nameof(PotionLightBlue)}
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

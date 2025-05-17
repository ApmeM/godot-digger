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
            {nameof(PlantBlue), nameof(PotionBlue)},
            {nameof(PlantBrown), nameof(PotionBrown)},
            {nameof(PlantGreen), nameof(PotionGreen)},
            {nameof(PlantOrange), nameof(PotionOrange)},
            {nameof(PlantRed), nameof(PotionRed)},
            {nameof(PlantViolet), nameof(PotionViolet)},
            {nameof(PlantWhite), nameof(PotionWhite)},
            {nameof(PlantYellow), nameof(PotionYellow)},
        };
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

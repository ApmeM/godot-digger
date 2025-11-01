using System.Threading.Tasks;
using Godot;

[SceneReference("PotionLightBlue.tscn")]
public partial class PotionLightBlue
{
    public PotionLightBlue()
    {
        Price = 1;
        MaxCount = 1;
        ItemType = ItemType.Potion;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

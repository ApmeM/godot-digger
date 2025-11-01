using System.Threading.Tasks;
using Godot;

[SceneReference("Bread.tscn")]
public partial class Bread
{
    public Bread()
    {
        Price = 30;
        MaxCount = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

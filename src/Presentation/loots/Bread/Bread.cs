using System.Threading.Tasks;
using Godot;

[SceneReference("Bread.tscn")]
public partial class Bread
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
}

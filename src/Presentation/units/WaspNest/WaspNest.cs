using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("WaspNest.tscn")]
public partial class WaspNest
{
    public WaspNest()
    {
        this.HP = 12;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }

    public override void GotHit(BaseUnit from, int attackPower)
    {
        base.GotHit(from, attackPower);

        var instance = Instantiator.CreateUnit("Wasp");
        instance.Position = this.Position;
        instance.LevelPath = this.LevelPath;
        instance.AggroAgainst = this.AggroAgainst;
        foreach (var group in this.GetGroups().Cast<string>().Intersect(Groups.GroupsListForAggro))
        {
            instance.AddToGroup(group);
        }
        
        this.GetParent().AddChild(instance);

        if (this.HP <= 0)
        {
            level.FloatingTextManagerControl.ShowValue(Instantiator.CreateBuff(Buff.Dead), this.Position);
        }
    }
}

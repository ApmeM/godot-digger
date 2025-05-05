using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Wasp.tscn")]
public partial class Wasp
{
    public Wasp()
    {
        this.HP = 20;
        this.AttackPower = 1;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.texture.Connect(CommonSignals.Pressed, this, nameof(UnitClicked));
    }

    [Export]
    public float Speed = 100;

    [Export]
    public float MoveDelay = 0.2f;

    private Vector2? path;

    private float currentMoveDelay;
    private HashSet<Floor> floors = new HashSet<Floor> { Floor.Ground, Floor.Road, Floor.Water };

    public override void _Process(float delta)
    {
        base._Process(delta);

        currentMoveDelay += delta;
        if (currentMoveDelay <= MoveDelay)
        {
            return;
        }

        if (path == null)
        {
            this.path = this.GetPathToOtherGroup(floors) ??
                        this.GetPathToRandomLocation(floors);
            if (this.path == null)
            {
                path = null;
                currentMoveDelay = 0;
                return;
            }
            path = level.MapToWorld(path.Value);
        }

        if (base.TryAttackAt(path.Value))
        {
            currentMoveDelay = 0;
        }

        if (base.MoveUnit(path.Value, Speed * delta))
        {
            path = null;
            currentMoveDelay = 0;
            return;
        }
    }

    public override void GotHit(BaseUnit from, int attackPower)
    {
        base.GotHit(from, attackPower);
        if (this.HP <= 0)
        {
            level.FloatingTextManagerControl.ShowValue(Instantiator.CreateBuff(Buff.Dead), this.Position);
        }
    }
}

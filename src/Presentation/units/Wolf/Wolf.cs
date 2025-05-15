using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Wolf.tscn")]
public partial class Wolf
{
    public Wolf()
    {
        HP = 30;
        AttackPower = 5;
        Loot = new List<string> { nameof(WolfSkin) };
        AggroAgainst = Groups.GroupsListForAggro.ToHashSet();
    }


    [Export]
    public float Speed = 100;

    [Export]
    public float MoveDelay = 0;
    
    [Export]
    public float AttackDelay = 5;

    private Vector2? path;

    private float currentMoveDelay;
    private HashSet<Floor> floors = new HashSet<Floor> { Floor.Ground, Floor.Road };

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouse && mouse.IsPressed() && !mouse.IsEcho() && (ButtonList)mouse.ButtonIndex == ButtonList.Left)
        {
            var size = animatedSprite.Frames.GetFrame(animatedSprite.Animation, animatedSprite.Frame).GetSize();
            var rect = new Rect2(this.animatedSprite.Position, size);
            var mousePos = this.GetLocalMousePosition();
            
            if (rect.HasPoint(mousePos))
            {
                this.GetTree().SetInputAsHandled();
                this.UnitClicked();
            }
        }
    }
    
    public override void _Process(float delta)
    {
        base._Process(delta);

        currentMoveDelay -= delta;
        if (currentMoveDelay > 0)
        {
            return;
        }

        if (path == null)
        {
            this.path = this.GetPathToOtherGroup(floors, 10) ??
                        this.GetPathToRandomLocation(floors);
            if (this.path == null)
            {
                path = null;
                currentMoveDelay = MoveDelay;
                return;
            }
            path = level.MapToWorld(path.Value);
        }

        if (base.TryAttackAt(path.Value))
        {
            currentMoveDelay = AttackDelay;
        }

        // var stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        // stateMachine.Travel("Move");

        var dir = (path.Value - this.Position).Normalized();
        this.animationTree.Set("parameters/Move/blend_position", new Vector2(dir.x, -dir.y));

        if (base.MoveUnit(path.Value, Speed * delta))
        {
            path = null;
            currentMoveDelay = MoveDelay;
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

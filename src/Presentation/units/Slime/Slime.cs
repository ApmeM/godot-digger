using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Slime.tscn")]
public partial class Slime
{
    public Slime()
    {
        this.HP = 2;
        this.AttackPower = 1;
    }

    [Export]
    public float Speed = 100;

    [Export]
    public float MoveDelay = 1;

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

        currentMoveDelay += delta;
        if (currentMoveDelay <= MoveDelay)
        {
            return;
        }

        if (path == null)
        {
            this.path = this.GetPathToLoot(floors) ?? this.GetPathToRandomLocation(floors);
            if (this.path == null)
            {
                path = null;
                currentMoveDelay = 0;
                return;
            }
            path = level.MapToWorld(path.Value);
        }

        if (base.MoveUnit(path.Value, Speed * delta))
        {
            var loots = this.GetTree()
                .GetNodesInGroup(Groups.Loot)
                .Cast<BaseLoot>()
                .Where(a => level.WorldToMap(a.Position) == level.WorldToMap(this.Position))
                .ToList();

            foreach (var l in loots)
            {
                this.Loot.Add(l.LootName);
                l.QueueFree();
            }

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

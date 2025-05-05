using System.Collections.Generic;
using Godot;
using GodotDigger.Presentation.Utils;

[SceneReference("Fish.tscn")]
public partial class Fish
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    [Export]
    public float Speed = 100;

    [Export]
    public float MoveDelay = 1;

    private Vector2? path;

    private float currentMoveDelay;
    private HashSet<Floor> floors = new HashSet<Floor> { Floor.Water };

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
            this.path = this.GetPathToRandomLocation(floors);
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
            path = null;
            currentMoveDelay = 0;
            return;
        }
    }
}

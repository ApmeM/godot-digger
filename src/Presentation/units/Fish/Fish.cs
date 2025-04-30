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

    public override void _Process(float delta)
    {
        base._Process(delta);

        currentMoveDelay += delta;
        if (currentMoveDelay <= MoveDelay)
        {
            return;
        }

        var level = this.GetNode<BaseLevel>(this.LevelPath);
        if (path == null)
        {
            var floors = new HashSet<Floor> { Floor.Water };
            this.path = this.GetPathToRandomLocation(floors);
            if (this.path == null)
            {
                path = null;
                currentMoveDelay = 0;
                return;
            }
            path = level.FloorMap.MapToWorld(path.Value);
        }

        if (base.MoveUnit(path.Value, Speed * delta))
        {
            path = null;
            currentMoveDelay = 0;
            return;
        }
    }
}

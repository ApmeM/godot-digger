using Godot;
using System;

[SceneReference("BaseProjectile.tscn")]
public partial class BaseProjectile
{
    [Export]
    public float Speed = 1000;

    private Func<Vector2> to;
    private Action onHit;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        var dir = to() - this.Position;
        var dist = Speed * delta;
        if (dir.LengthSquared() > dist * dist)
        {
            this.Rotation = dir.Angle();
            this.Position += dist * dir.Normalized();
            return;
        }

        this.Position = to();
        // TODO: Animate BOOM
        onHit?.Invoke();
        this.QueueFree();
    }

    public void Shoot(Vector2 from, BaseUnit toUnit, Action onHit)
    {
        this.Position = from;
        this.Rotation = (toUnit.Position - from).Angle();
        var lastSeen = toUnit.Position;
        this.to = () =>
        {
            if (Godot.Object.IsInstanceValid(toUnit))
            {
                lastSeen = toUnit.Position;
                return toUnit.Position;
            }
            this.to = () => lastSeen;
            return lastSeen;
        };

        this.onHit = onHit;
    }
}

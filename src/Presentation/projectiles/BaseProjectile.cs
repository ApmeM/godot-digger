using Godot;
using System;

[SceneReference("BaseProjectile.tscn")]
public partial class BaseProjectile
{
    [Export]
    public float Speed = 1000;

    [Export]
    public BaseUnit toUnit;

    [Export]
    public Vector2 toPoint;

    [Export]
    public BaseUnit fromUnit;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        this.toPoint = Godot.Object.IsInstanceValid(this.toUnit) ? this.toUnit.Position : toPoint;

        var dir = this.toPoint - this.Position;
        var dist = Speed * delta;
        if (dir.LengthSquared() > dist * dist)
        {
            this.Rotation = dir.Angle();
            this.Position += dist * dir.Normalized();
            return;
        }

        this.Position = this.toPoint;
        // TODO: Animate BOOM
        this.QueueFree();

        if (!Godot.Object.IsInstanceValid(this.toUnit))
        {
            return;
        }

        this.toUnit.GotHit(fromUnit);
    }

    public void Shoot(BaseUnit fromUnit, BaseUnit toUnit)
    {
        this.toUnit = toUnit;
        this.fromUnit = fromUnit;
        this.toPoint = this.toUnit.Position;

        this.Position = this.fromUnit.Position;
        this.Rotation = (this.toUnit.Position - this.fromUnit.Position).Angle();
    }
}

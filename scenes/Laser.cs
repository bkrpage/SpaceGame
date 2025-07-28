using Godot;
using System;

public partial class Laser : Area2D
{

	[Export] public float Speed = 500f;
	
	[Export] public Vector2 ShooterVelocity =  new(0,0);

	private Sprite2D _laserSprite;
	public override void _Ready()
	{
		_laserSprite =  GetNode<Sprite2D>("LaserSprite");

		var tween = CreateTween();
		tween.TweenProperty(_laserSprite, "scale", new Vector2(1f, 1f), 0.1).From(new Vector2(0, 0));

	}
	public override void _Process(double delta)
	{
		var deltaf = (float) delta;
		
		var shooterVelocityY = new Vector2(0, ShooterVelocity.Y);
		var baseVelocity = new Vector2(0, -1f) * Speed;
		var modifiedVelocity = baseVelocity + shooterVelocityY;
		if (modifiedVelocity.Length() < 300f && modifiedVelocity != Vector2.Zero)
		{
			modifiedVelocity = modifiedVelocity.Normalized() * 300f;
		}
		
		Position += modifiedVelocity * deltaf;
		
		// Yeet when offscreen.
		if (Position.Y < -200f)
		{
			CallDeferred("queue_free");;
		}
		
	}
}

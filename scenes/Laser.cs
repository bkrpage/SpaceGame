using Godot;
using System;

public partial class Laser : Area2D
{

	[Export] public int BaseDamage = 100;

	[Export] public float Speed = 500f;
	
	public Vector2 ShooterVelocity =  new(0,0);
	
	public int CalculatedDamage => BaseDamage;

	private Sprite2D _laserSprite;

	private Vector2 _modifiedVelocity;
	
	public override void _Ready()
	{
		_getNodes();
		_initialiseVelocity();

	}
	
	public override void _Process(double delta)
	{
		var deltaf = (float) delta;
		Position += _modifiedVelocity * deltaf;
		_despawnIfOffScreen();
		
	}

	public void Despawn()
	{
		CallDeferred("queue_free");
	}

	private void _getNodes()
	{
		_laserSprite =  GetNode<Sprite2D>("LaserSprite");
	}

	private bool _isOffscreen()
	{
		return Position.Y < -200f;
	}

	private void _despawnIfOffScreen()
	{
		if (_isOffscreen())
		{
			Despawn();
		}
	}

	private void _initialiseVelocity()
	{
		var shooterVelocityY = new Vector2(0, ShooterVelocity.Y);
		var baseVelocity = new Vector2(0, -1f) * Speed;
		_modifiedVelocity = baseVelocity + shooterVelocityY;
		if (_modifiedVelocity.Length() < 300f && _modifiedVelocity != Vector2.Zero)
		{
			_modifiedVelocity = _modifiedVelocity.Normalized() * 300f;
		}
	}
	

	private void _initialiseTween()
	{
		var tween = CreateTween();
		tween.TweenProperty(_laserSprite, "scale", new Vector2(1f, 1f), 0.1).From(new Vector2(0, 0));
	}
}

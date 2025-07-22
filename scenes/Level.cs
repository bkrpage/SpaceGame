using Godot;
using System;

public partial class Level : Node2D
{
	// Exports
	[Export] public double MeteorTimerTimout = 1.0;
	[Export] public int StarsAmount = 35;
	
	// Scenes
	private PackedScene _meteorScene = GD.Load<PackedScene>("res://scenes/meteor.tscn");
	private PackedScene _laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
	private PackedScene _starScene = GD.Load<PackedScene>("res://scenes/star.tscn");
	
	// Instance variables
	private RandomNumberGenerator _rng = new();

	private Vector2 _screenSize;
	
	// Nodes
	private Player _playerNode;
	private Node2D _lasersNode;
	private Node2D _meteorsNode;
	private Timer _meteorTimerNode;
	private Node2D _starsNode;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;
		
		_getNodes();
		
		_registerMeteorTimer();
		_registerShootLaser();

		_randomiseStars();
	}


	/// <summary>
	/// Get the nodes used within the level and assign to the instance variables.
	/// </summary>
	private void _getNodes()
	{
		_meteorsNode = GetNode<Node2D>("Meteors");
		_playerNode = GetNode<Player>("Player");
		_meteorsNode = GetNode<Node2D>("Meteors");
		_meteorTimerNode =  GetNode<Timer>("MeteorTimer");
		_lasersNode =  GetNode<Node2D>("Lasers");
		_starsNode =  GetNode<Node2D>("Stars");
	}

	private void _registerShootLaser()
	{
		_playerNode.ShootLaser += _onShootLaser;
	}

	private void _registerMeteorTimer()
	{
		_meteorTimerNode.WaitTime = MeteorTimerTimout;
		_meteorTimerNode.Start();
		_meteorTimerNode.Timeout += _onMeteorTimerTimeout;	
	}
	
	private void _onShootLaser(Vector2 position, Vector2 originatingVelocity)
	{
		var laser =  _laserScene.Instantiate() as Laser;
		laser.Position = position;
		laser.ShooterVelocity = originatingVelocity;
		_lasersNode.AddChild(laser);
	}

	private void _onMeteorTimerTimeout()
	{
		var meteor = _meteorScene.Instantiate() as Meteor;
		// If we want to make a series of 'special' meteors, we can do that using the below:
		// meteor.BaseSpeed = 5000f;
		_meteorsNode.AddChild(meteor);
	}

	private void _randomiseStars()
	{
		for (int i = 0; i < StarsAmount; i++)
		{
			var star = _starScene.Instantiate() as Node2D;
			var sprite = star.GetChild<AnimatedSprite2D>(0);
			
			var frameCount = sprite.SpriteFrames.GetFrameCount("default");
			sprite.SetFrame(_rng.RandiRange(0, frameCount - 1));
			sprite.SpeedScale = _rng.RandfRange(0.5f, 1.5f);
			
			var starPosition = new Vector2(_rng.RandiRange(0, (int) _screenSize.X ), _rng.RandiRange(0, (int) _screenSize.Y));
			star.Position = starPosition;

			var starScale = _rng.RandfRange(0.5f, 1.5f);
			star.Scale = new Vector2(starScale, starScale);
			
			_starsNode.AddChild(star);
		}
		
		
	}
}

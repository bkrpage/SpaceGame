using Godot;
using System;

public partial class Level : Node2D
{
	// Exports
	[Export] public double MeteorTimerTimout = 1.0;
	[Export] public int StarsAmount = 35;
	
	[Export] public PackedScene GameOverScene = GD.Load<PackedScene>("res://scenes/game_over.tscn");
	
	// Scenes
	private PackedScene _meteorScene = GD.Load<PackedScene>("res://scenes/meteor.tscn");
	private PackedScene _laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
	private PackedScene _starScene = GD.Load<PackedScene>("res://scenes/star.tscn");
	
	// Instance variables
	private RandomNumberGenerator _rng = new();
	private Vector2 _screenSize;

	// TODO I don't like this here, I feel like it should be in the player itself
	private int _playerHealth = 5;
	
	// Nodes
	private Global _globalNode;
	private Player _playerNode;
	private Node2D _lasersNode;
	private Node2D _meteorsNode;
	private Timer _meteorTimerNode;
	private Node2D _starsNode;
	private AudioStreamPlayer2D _explosionStreamPlayerNode;
	private AudioStreamPlayer2D _damageStreamPlayerNode;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;

		_getNodes();

		_registerMeteorTimer();
		_registerShootLaser();

		_randomiseStars();
		_setHealthUi();
		_setScoreToZero();
	}


	/// <summary>
	/// Get the nodes used within the level and assign to the instance variables.
	/// </summary>
	private void _getNodes()
	{
		_globalNode = GetNode<Global>("/root/Global");
		_meteorsNode = GetNode<Node2D>("Meteors");
		_playerNode = GetNode<Player>("Player");
		_meteorTimerNode =  GetNode<Timer>("MeteorTimer");
		_lasersNode =  GetNode<Node2D>("Lasers");
		_starsNode =  GetNode<Node2D>("Stars");
		_explosionStreamPlayerNode = GetNode<AudioStreamPlayer2D>("ExplosionSound");
		_damageStreamPlayerNode = GetNode<AudioStreamPlayer2D>("DamageSound");
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
		if (_meteorScene.Instantiate() is not Meteor meteor)
		{
			return;
		}
		// If we want to make a series of 'special' meteors, we can do that using the below:
		// meteor.BaseSpeed = 5000f;
		_meteorsNode.AddChild(meteor);

		meteor.Connect("Collision", new Callable(this, nameof(_onMeteorCollision)));
		meteor.Connect("Destroyed", new Callable(this, nameof(_onMeteorDestroyed)));
	}

	private void _onMeteorCollision()
	{
		_damageStreamPlayerNode.Play();
		// Or could instead do:
		// _playerNode.PlayCollisionSound();
		_playerHealth--;
		GetTree().CallGroup("ui", "SetHealth", _playerHealth);
		if (_playerHealth <= 0)
		{
			GetTree().ChangeSceneToPacked(GameOverScene);
		}

	}

	private void _onMeteorDestroyed()
	{
		_explosionStreamPlayerNode.Play();
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
	
	private void _setHealthUi()
	{
		GetTree().CallGroup("ui", "SetHealth", _playerHealth);
	}
	
	private void _setScoreToZero()
	{
		_globalNode.Score = 0;
	}
	
}

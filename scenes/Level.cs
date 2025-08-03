using Godot;
using System;

public partial class Level : Node2D
{
	// Exports
	[Export] public double MeteorTimerTimout = 1.0;
	[Export] public int StarsAmount = 35;
	[Export] public double ScoreTimerTimeout = 0.5;

	// Scenes
	private PackedScene _gameOverScene = GD.Load<PackedScene>("res://scenes/game_over.tscn");
	private PackedScene _meteorScene = GD.Load<PackedScene>("res://scenes/meteor.tscn");
	private PackedScene _laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
	private PackedScene _starScene = GD.Load<PackedScene>("res://scenes/star.tscn");
	private PackedScene _floatingTextScene = GD.Load<PackedScene>("res://scenes/floating_text.tscn");
	
	// Instance variables
	private Vector2 _screenSize;

	// TODO I don't like this here, I feel like it should be in the player itself
	private int _playerHealth = 5;
	
	// Nodes
	private GameState _gameStateNode;
	private Player _playerNode;
	private Node2D _lasersNode;
	private Node2D _meteorsNode;
	private Node2D _starsNode;
	private Node2D _floatingScoreTextNode;
	private Timer _meteorTimerNode;
	private AudioStreamPlayer2D _explosionStreamPlayerNode;
	private Timer _scoreTimerNode;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;

		_getNodes();
		
		// TODO cleanup
		GameState.GenerateNewSeed();
		
		_registerScoreTimer();

		_registerMeteorTimer();
		_registerShootLaser();

		_randomiseStars();
		_setHealthUi();
		_setScoreToZero();
	}

	public override void _ExitTree()
	{
		_scoreTimerNode.Timeout -= _onScoreTimerTimeout;
		base._ExitTree();
	}


	/// <summary>
	/// Get the nodes used within the level and assign to the instance variables.
	/// </summary>
	private void _getNodes()
	{
		_gameStateNode = GetNode<GameState>("/root/GameState");
		_playerNode = GetNode<Player>("Player");
		_meteorsNode = GetNode<Node2D>("Meteors");
		_lasersNode =  GetNode<Node2D>("Lasers");
		_starsNode =  GetNode<Node2D>("Stars");
		_floatingScoreTextNode =  GetNode<Node2D>("FloatingScoreText");
		_meteorTimerNode =  GetNode<Timer>("MeteorTimer");
		_explosionStreamPlayerNode = GetNode<AudioStreamPlayer2D>("ExplosionSound");
		_scoreTimerNode =  GetNode<Timer>("ScoreTimer");
	}
	
	private void _registerScoreTimer()
	{
		_scoreTimerNode.WaitTime = ScoreTimerTimeout;
		_scoreTimerNode.Start();
		_scoreTimerNode.Timeout += _onScoreTimerTimeout;
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
	

	private void _onScoreTimerTimeout()
	{
		_gameStateNode.IncrementScore();
	}

	private void _onMeteorCollision()
	{
		_playerNode.PlayDamageSound();
		_playerNode.Health--;
		GetTree().CallGroup("ui", "SetHealth", _playerNode.Health);
		if (_playerNode.Health <= 0)
		{
			GetTree().ChangeSceneToPacked(_gameOverScene);
		}

	}

	private void _onMeteorDestroyed(Vector2 position, int score)
	{
		
		_displayScoreText(position, "+ " + score, 0.4f);
		_gameStateNode.UpdateScoreBy(score);
		_playExplosionSound();
	}

	private async void _displayScoreText(Vector2 position, string scoreText, float duration)
	{
		var floatingText = _floatingTextScene.Instantiate() as FloatingText;
		if (floatingText == null) return;
		floatingText.Position = position;
		floatingText.SetText(scoreText);		
		var tween = CreateTween();
		tween.TweenProperty(floatingText, "scale", new Vector2(2.0f, 2.0f), duration / 2)
			.From(new Vector2(1f, 1f));
		tween.TweenProperty(floatingText, "scale", new Vector2(0f, 0f), duration);

		_floatingScoreTextNode.AddChild(floatingText);
		await ToSignal(tween, "finished");
		floatingText.CallDeferred("queue_free");;
	}

	private void _playExplosionSound()
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
			sprite.SetFrame(GameState.Rng.RandiRange(0, frameCount - 1));
			sprite.SpeedScale = GameState.Rng.RandfRange(0.5f, 1.5f);
			
			var starPosition = new Vector2(GameState.Rng.RandiRange(0, (int) _screenSize.X ), GameState.Rng.RandiRange(0, (int) _screenSize.Y));
			star.Position = starPosition;

			var starScale = GameState.Rng.RandfRange(0.5f, 1.5f);
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
		_gameStateNode.Score = 0;
	}
	
}

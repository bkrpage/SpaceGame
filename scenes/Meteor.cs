using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Meteor : Area2D
{

	
	// Display
	[Export] public string TextureResourcePath = "res://assets/graphic/Meteors/";
	[Export] public string TextureNameTemplate = "meteor{meteorColor}_big{meteorShape}.png";
	[Export] public string[] TextureColors = ["Grey", "Brown"];
	[Export] public int TextureShapeVariationAmount = 4;
	[Export] public float BaseScale = 1f;
	[Export] public float ScaleRandomFactor = 0.5f;
	
	// Movement
	[Export] public float BaseSpeed = 400f;
	[Export] public float SpeedRandomFactor = 0.2f;
	[Export] public float MaxYVectorVariation = 0.5f;
	[Export] public float MaxRotationSpeed = 8.5f;

	// Score
	[Export] public int BaseScore = 50;
	[Export] public float SpeedScoreInfluence = 0.5f;
	[Export] public float ScaleScoreInfluence = 0.5f;

	// Score
	[Export] public int BaseHealth = 100;
	[Export] public float SpeedHealthInfluence = 0f;
	[Export] public float ScaleHealthInfluence = 5f;
	
	// Signals
	[Signal] public delegate void CollisionEventHandler();
	[Signal] public delegate void DestroyedEventHandler(Vector2 position, int score);

	// Display
	private string _meteorColor;
	private int _meteorShape;
	private string _textureName;
	private float _scale;
	
	// Movement
	private float _speed;
	private float _yVariation;
	private float _rotationSpeed;
	
	// Other
	private int _health;
	private Vector2 _viewportSize;
	private bool _destroyable;
	
	
	// nodes
	private GameState _gameStateNode;
	private Sprite2D _spriteNode;


	public override void _Ready()
	{
		_getNodes();
		_getViewportSize();
		_setupSpriteAndCollisions();
		_randomiseScale();
		_initialisePosition();
		_randomiseSpeedAndRotation();
		_initialiseHealth();
		_registerSignals();
	}

	private void _getNodes()
	{
		_gameStateNode = GetNode<GameState>("/root/GameState");
		_spriteNode = GetNode<Sprite2D>("MeteorSprite");
	}
	
	public override void _Process(double delta)
	{
		var deltaf = (float) delta;
		_getViewportSize();
		_processPositionAndRotation(deltaf);
		_determineDestroyable();
		_despawnIfOffScreen();
	}

	private void _getViewportSize()
	{
		_viewportSize = WindowManager.ViewportSize;
	}

	private void _initialiseHealth()
	{
		_health = _calculateHealth();
		//Debug purposes
		Label label = new Label();
		label.Text = $"{_health}";
		label.Position = new Vector2(0, 0);
		AddChild(label);
		//End debug purposes
	}
	
	private void _determineDestroyable()
	{
		if (_destroyable) return;
		var meteorRect = _spriteNode.GetRect();
		var meteorGlobalPosition = GlobalPosition;

		_destroyable = (meteorGlobalPosition.Y - meteorRect.Size.Y) > -meteorRect.Size.Y;
	}

	private void _processPositionAndRotation(float deltaf)
	{
		Position += new Vector2(_yVariation, 1f) * _speed * deltaf;
		Rotate(_rotationSpeed * deltaf);
	}

	private void _despawnIfOffScreen()
	{
		if (_isOffScreen())
		{
			CallDeferred("queue_free");
		}
	}

	private bool _isOffScreen()
	{
		return Position.Y > _viewportSize.Y + 200f;
	}
	
	private void _setupSpriteAndCollisions()
	{
		_meteorColor = TextureColors[GameState.Rng.RandiRange(0, TextureColors.Length - 1)];
		_meteorShape = GameState.Rng.RandiRange(1, TextureShapeVariationAmount);
		
		_textureName = TextureNameTemplate
			.Replace("{meteorColor}", _meteorColor)
			.Replace("{meteorShape}", _meteorShape.ToString());

		
		_spriteNode.Texture = GD.Load<Texture2D>(TextureResourcePath + _textureName);
		
		//Now enable corresponding collision
		for (var i = 1; i <= TextureShapeVariationAmount; i++)
		{
			var meteorCollisionName = $"MeteorCollision{i}";
			var collisionShape = GetNode<CollisionPolygon2D>(meteorCollisionName);
			collisionShape.Disabled = i != _meteorShape;
		}
	}

	private void _initialisePosition()
	{
		var initialPositionX = GameState.Rng.RandiRange(0, (int) _viewportSize.X);
		var initialPositionY = GameState.Rng.RandiRange(-150, -50);
		Position = new Vector2(initialPositionX, initialPositionY);
	}
	
	private void _randomiseSpeedAndRotation()
	{
		var speedVariation = 1.0f + GameState.Rng.RandfRange(-SpeedRandomFactor, SpeedRandomFactor);
		_speed = BaseSpeed * speedVariation;
		_yVariation = GameState.Rng.RandfRange(-MaxYVectorVariation, MaxYVectorVariation);
		
		_rotationSpeed = GameState.Rng.RandfRange(-MaxRotationSpeed, MaxRotationSpeed);
	}

	private void _randomiseScale()
	{
		var scaleVariation = 1.0f + GameState.Rng.RandfRange(-ScaleRandomFactor, ScaleRandomFactor);
		_scale = BaseScale *  scaleVariation;

		Scale *= _scale;
	}

	private int _calculateScore()
	{
		var speedFactor = _speed / BaseSpeed;
		var scaleFactor = BaseScale / _scale;

		var speedMultiplier = 1.0f + ((speedFactor - 1.0f) * SpeedScoreInfluence);
		var scaleMultiplier = 1.0f + ((scaleFactor - 1.0f) * ScaleScoreInfluence);

		return Mathf.RoundToInt(BaseScore * speedMultiplier * scaleMultiplier);
	}

	private int _calculateHealth()
	{
		const float minFactor = 1f;
		var speedFactor = Math.Max(_speed / BaseSpeed, minFactor);
		var scaleFactor = Math.Max(_scale / BaseScale, minFactor);;

		var speedMultiplier = 1.0f + ((speedFactor - 1.0f) * SpeedHealthInfluence);
		var scaleMultiplier = 1.0f + ((scaleFactor - 1.0f) * ScaleHealthInfluence);

		return Mathf.RoundToInt(BaseScore * speedMultiplier * scaleMultiplier);
	}

	private void _registerSignals()
	{
		BodyEntered += _OnBodyEntered;
		AreaEntered += _OnAreaEntered;
	}


	// Player is a Body, so this is called.
	private void _OnBodyEntered(Node2D body)
	{
		EmitSignal(SignalName.Collision);
	}

	// Laser is an Area, so this is called.
	private void _OnAreaEntered(Node2D body)
	{
		if (body is not Laser laser) return;
		
		_health -= laser.CalculatedDamage;
		if (_health <= 0)
		{
			_destroyIfDestroyable();
		}

		laser.Despawn();
	}

	private void _destroyIfDestroyable()
	{
		if (!_destroyable) return;
		EmitSignal(SignalName.Destroyed, Position, _calculateScore());
		// Could instead play the sound from here, hide node and collision, and then await for x seconds for
		// sound to play. This means the sound will be positionally correct for where it was destroyed.
		
		CallDeferred("queue_free");
	}
}

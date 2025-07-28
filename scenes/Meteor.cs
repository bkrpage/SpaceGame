using Godot;
using System;

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
	[Export] public float MaxRotationSpeedDeg = 500f;

	// Score
	[Export] public int BaseScore = 50;
	[Export] public float SpeedScoreInfluence = 0.5f;
	[Export] public float ScaleScoreInfluence = 0.5f;
	
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
	private float _rotationSpeedDeg;
	
	// Other
	private float _score;

	private bool _destroyable;
	
	// nodes
	private Global _globalNode;
	private Sprite2D _spriteNode;


	public override void _Ready()
	{
		_getNodes();
		_setupSpriteAndCollisions();
		_randomiseScale();
		_initialisePosition();
		_randomiseSpeedAndRotation();
		_calculateScoreGiven();
		_registerSignals();
	}

	private void _getNodes()
	{
		_globalNode = GetNode<Global>("/root/Global");
		_spriteNode = GetNode<Sprite2D>("MeteorSprite");
	}
	
	public override void _Process(double delta)
	{
		var deltaf = (float) delta;
		_processPositionAndRotation(deltaf);
		_determineDestroyable();
		_despawnIfOffScreen();
	}
	
	private void _determineDestroyable()
	{
		if (_destroyable) return;
		
		var viewportSize = WindowManager.ViewportSize;
		var meteorRect = _spriteNode.GetRect();
		var meteorGlobalPosition = GlobalPosition;

		_destroyable = (meteorGlobalPosition.Y - meteorRect.Size.Y) > -meteorRect.Size.Y;
	}

	private void _processPositionAndRotation(float deltaf)
	{
		Position += new Vector2(_yVariation, 1f) * _speed * deltaf;
		RotationDegrees += _rotationSpeedDeg * deltaf;
	}

	private void _despawnIfOffScreen()
	{
		if (Position.Y > WindowManager.ViewportSize.Y + 200f)
		{
			CallDeferred("queue_free");
		}
	}
	
	private void _setupSpriteAndCollisions()
	{
		_meteorColor = TextureColors[_globalNode.Rng.RandiRange(0, TextureColors.Length - 1)];
		_meteorShape = _globalNode.Rng.RandiRange(1, TextureShapeVariationAmount);
		
		_textureName = TextureNameTemplate
			.Replace("{meteorColor}", _meteorColor)
			.Replace("{meteorShape}", _meteorShape.ToString());

		var sprite = GetNode<Sprite2D>("MeteorSprite");
		sprite.Texture = GD.Load<Texture2D>(TextureResourcePath + _textureName);
		
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
		var initialPositionX = _globalNode.Rng.RandiRange(0, (int) WindowManager.ViewportSize.X);
		var initialPositionY = _globalNode.Rng.RandiRange(-150, -50);
		Position = new Vector2(initialPositionX, initialPositionY);
	}
	
	private void _randomiseSpeedAndRotation()
	{
		var speedVariation = 1.0f + _globalNode.Rng.RandfRange(-SpeedRandomFactor, SpeedRandomFactor);
		_speed = BaseSpeed * speedVariation;
		_yVariation = _globalNode.Rng.RandfRange(-MaxYVectorVariation, MaxYVectorVariation);
		
		_rotationSpeedDeg = _globalNode.Rng.RandfRange(-MaxRotationSpeedDeg, MaxRotationSpeedDeg);
	}

	private void _randomiseScale()
	{
		var scaleVariation = 1.0f + _globalNode.Rng.RandfRange(-ScaleRandomFactor, ScaleRandomFactor);
		_scale = BaseScale *  scaleVariation;

		Scale *= _scale;
	}

	private void _calculateScoreGiven()
	{

		// Normalize to base (e.g. 1.0 speed & scale should give multiplier of 1)
		var speedFactor = _speed / BaseSpeed;
		var scaleFactor = BaseScale / _scale; // smaller scale = higher factor

		var speedMultiplier = 1.0f + ((speedFactor - 1.0f) * SpeedScoreInfluence);
		var scaleMultiplier = 1.0f + ((scaleFactor - 1.0f) * ScaleScoreInfluence);

		_score = Mathf.RoundToInt(BaseScore * speedMultiplier * scaleMultiplier);
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
		// TODO - Multiple hit meteors. Or could be 'health' based and health removal is determined by the
		// impact velocity.
		_destroyIfDestroyable();
		// Get rid of laser that destroyed it
		body.QueueFree();
	}

	private void _destroyIfDestroyable()
	{
		if (!_destroyable) return;
		EmitSignal(SignalName.Destroyed, Position, _score);
		// Could instead play the sound from here, hide node and collision, and then await for x seconds for
		// sound to play. This means the sound will be positionally correct for where it was destroyed.
		
		CallDeferred("queue_free");
	}
}

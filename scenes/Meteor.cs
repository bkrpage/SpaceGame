using Godot;
using System;

public partial class Meteor : Area2D
{

	[Export] public float BaseSpeed = 400f;
	[Export] public float SpeedRandomFactor = 0.2f;

	[Export] public int BaseScore = 50;
	[Export] public float SpeedInfluence = 0.5f;
	[Export] public float ScaleInfluence = 0.5f;
	
	[Export] public float BaseScale = 1f;
	[Export] public float ScaleRandomFactor = 0.5f;
	
	[Export] public float MaxYVectorVariation = 0.5f;
	
	[Export] public float MaxRotationSpeedDeg = 500f;

	[Export] public string TextureResourcePath = "res://assets/graphic/Meteors/";
	[Export] public string TextureNameTemplate = "meteor{meteorColor}_big{meteorShape}.png";

	[Export] public string[] TextureColors = ["Grey", "Brown"];
	[Export] public int TextureShapeVariationAmount = 4;

	private RandomNumberGenerator _rng = new();

	private string _meteorColor;
	private int _meteorShape;
	private string _textureName;
	
	private float _speed;
	private float _yVariation;
	private float _rotationSpeedDeg;

	private float _scale;

	private float _score;
	
	[Signal]
	public delegate void CollisionEventHandler();

	[Signal]
	public delegate void DestroyedEventHandler(Vector2 position, int score);

	public override void _Ready()
	{
		_getNodes();
		_setupSpriteAndCollisions();
		_initialiseRandomScale();
		_initialisePosition();
		_initialiseSpeedAndRotation();
		_calculateScore();
		_registerSignals();
	}

	private void _getNodes()
	{
	}
	
	public override void _Process(double delta)
	{
		var deltaf = (float) delta;
		
		Position += new Vector2(_yVariation, 1f) * _speed * deltaf;
		
		RotationDegrees += _rotationSpeedDeg * deltaf;
		
		// Yeet when offscreen.
		if (Position.Y > WindowManager.ViewportSize.Y + 200f)
		{
			QueueFree();
		}
		
	}
	
	private void _setupSpriteAndCollisions()
	{
		_meteorColor = TextureColors[_rng.RandiRange(0, TextureColors.Length - 1)];
		_meteorShape = _rng.RandiRange(1, TextureShapeVariationAmount);
		
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
		var initialPositionX = _rng.RandiRange(0, (int) WindowManager.ViewportSize.X);
		var initialPositionY = _rng.RandiRange(-150, -50);
		Position = new Vector2(initialPositionX, initialPositionY);
	}
	
	private void _initialiseSpeedAndRotation()
	{
		var speedVariation = 1.0f + _rng.RandfRange(-SpeedRandomFactor, SpeedRandomFactor);
		_speed = BaseSpeed * speedVariation;
		_yVariation = _rng.RandfRange(-MaxYVectorVariation, MaxYVectorVariation);
		
		_rotationSpeedDeg = _rng.RandfRange(-MaxRotationSpeedDeg, MaxRotationSpeedDeg);
	}

	private void _initialiseRandomScale()
	{
		var scaleVariation = 1.0f + _rng.RandfRange(-ScaleRandomFactor, ScaleRandomFactor);
		_scale = BaseScale *  scaleVariation;

		Scale *= _scale;
	}

	private void _calculateScore()
	{

		// Normalize to base (e.g. 1.0 speed & scale should give multiplier of 1)
		var speedFactor = _speed / BaseSpeed;
		var scaleFactor = BaseScale / _scale; // smaller scale = higher factor

		var speedMultiplier = 1.0f + ((speedFactor - 1.0f) * SpeedInfluence);
		var scaleMultiplier = 1.0f + ((scaleFactor - 1.0f) * ScaleInfluence);

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
		EmitSignal(SignalName.Destroyed, Position, _score);
		// Could instead play the sound from here, hide node and collision, and then await for x seconds for soudn to play.
		// this means the sound will be positionally correct for where it was destroyed.
		GD.Print(_score);
		
		// Get rid of laser that destroyed it
		body.QueueFree();
		// and yeet self.
		QueueFree();
	}
}

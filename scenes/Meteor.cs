using Godot;
using System;

public partial class Meteor : Area2D
{

	[Export] public float BaseSpeed = 400f;
	[Export] public float SpeedRandomFactor = 0.2f;
	
	[Export] public float MaxYVariation = 0.5f;
	
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

	private int _windowWidth;
	private int _windowHeight;

	public override void _Ready()
	{
		
		//Set sprite
		_setupSpriteAndCollisions();
		
		//Set collision shape to match sprite
		
		// Get Window dimensions - We could probably get this from elsewhere instead of gettign this every time.
		_windowWidth = (int) GetViewport().GetVisibleRect().Size[0];
		_windowHeight = (int) GetViewport().GetVisibleRect().Size[1];
		
		// Place in initial Position;
		var initialPositionX = _rng.RandiRange(0, _windowWidth);
		var initialPositionY = _rng.RandiRange(-150, -50);
		Position = new Vector2(initialPositionX, initialPositionY);
		
		// Now set speed and direction variation;
		var speedVariation = 1.0f + _rng.RandfRange(-SpeedRandomFactor, SpeedRandomFactor);
		_speed = BaseSpeed * speedVariation;
		_yVariation = _rng.RandfRange(-MaxYVariation, MaxYVariation);
		
		// Now set rotation
		_rotationSpeedDeg = _rng.RandfRange(-MaxRotationSpeedDeg, MaxRotationSpeedDeg);
		
		// Setup event/signal handling.
		BodyEntered += _OnBodyEntered;
		
		// Print everything for debug
		// GD.Print(_windowWidth, "*", _windowHeight);
		// GD.Print("Texture name: ", _textureName);
		// GD.Print("Initial Position: ", Position);
		// GD.Print("Speed: ", (_speed));
		// GD.Print("Y Variation: ", (_yVariation));
		// GD.Print("Rotation Speed: ", (_rotationSpeedDeg));

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

	public override void _Process(double delta)
	{
		var deltaf = (float) delta;
		
		Position += new Vector2(_yVariation, 1f) * _speed * deltaf;
		
		RotationDegrees += _rotationSpeedDeg * deltaf;
		

		// Yeet when offscreen.
		if (Position.Y > _windowWidth + 200f)
		{
			GD.Print("Off Screen");
			QueueFree();
		}
		
	}

	private static void _OnBodyEntered(Node2D body)
	{
		GD.Print(body);
		
	}
}

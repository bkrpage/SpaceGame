using Godot;
using System;

public partial class Player : CharacterBody2D
{

	[Export] public float MaxSpeed = 250f;
	
	[Export] public float Friction = 0.2f;
	[Export] public float AccelerationFactor = 500f;
	[Export] public float DecelerationFactor = 200f;
	
	
	[Export] public float MaxSkew = 0.1f;
	[Export] public float SkewSpeed = 3f;
	
	[Export] public float RotationSpeed = 1f;
	[Export] public float MaxRotation =  0.1f;
	[Export] public StringName LeftInput = "left";
	[Export] public StringName RightInput = "right";
	[Export] public StringName ForwardInput = "forward";
	[Export] public StringName BackwardInput = "backward";
	
	[Export] public Texture2D PlayerTexture = GD.Load<Texture2D>("res://assets/graphic/playerShip1_orange.png");
	

	private Vector2 _velocity = Vector2.Zero;
	private Sprite2D _sprite;
	
	private float _targetSkew;

	private Vector2 _direction;
	
	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("PlayerSprite");
		_sprite.Texture = PlayerTexture;
	}

	public override void _Process(double delta)
	{
		var deltaf = (float)delta;
		
		_direction = Input.GetVector(LeftInput, RightInput, ForwardInput, BackwardInput);


		// going right
		if (_direction.X < 0)
		{
			Rotation -= RotationSpeed * deltaf;
			_targetSkew = -MaxSkew;
		}
		// going left
		else if (_direction.X > 0)
		{
			Rotation += RotationSpeed * deltaf;
			_targetSkew = MaxSkew;
		}    
		else
		{
			_targetSkew = 0f;
			// Smoothly rotate back to neutral when not pressing left/right
			Rotation = Mathf.MoveToward(Rotation, 0f, RotationSpeed * deltaf);
		}
		
		// Clamp the rotation so it doesn't exceed the max in either direction
		Rotation = Mathf.Clamp(Rotation, -MaxRotation, MaxRotation);
		
		_skew(deltaf);
		_squash();
		
		_move(deltaf);
	}

	private void _move(float deltaf)
	{
		Velocity = 
			_direction != Vector2.Zero ? 
				Velocity.MoveToward(_direction * MaxSpeed, AccelerationFactor * deltaf) :
				Velocity.MoveToward(Vector2.Zero, DecelerationFactor * Friction * deltaf);
		
		
		// Position += _velocity * deltaf;
		MoveAndSlide();
	}

	private void _squash()
	{
		// Squash sprite as well on horizontal axis
		var squash = 1f - Mathf.Abs(_targetSkew) * 0.2f;
		_sprite.Scale = new Vector2(squash, 1f);
	}

	private void _skew(float deltaf)
	{
		// Current transform
		var xform = _sprite.Transform;
		
		// Extract basis vectors
		var x = xform.X;
		var y = xform.Y;
		
		// Apply skew to x basis (skewing horizontally by changing the Y component of X)
		x.Y = Mathf.MoveToward(x.Y, _targetSkew, SkewSpeed * deltaf);
		
		// Reassign modified transform
		xform.X = x;
		_sprite.Transform = xform;
	}
}

using Godot;
using System;

public partial class Player : Node2D
{

	[Export] public float MaxSpeed = 250f;
	[Export] public float Acceleration = 500f;
	[Export] public float Deceleration = 1000f;
	
	
	[Export] public float MaxSkew = 0.2f;
	[Export] public float SkewSpeed = 5f;
	
	[Export] public float RotationSpeed = 1f;
	[Export] public float MaxRotation =  0.2f;
	[Export] public StringName LeftInput = "ui_left";
	[Export] public StringName RightInput = "ui_right";
	[Export] public StringName UpInput = "ui_up";
	[Export] public StringName DownInput = "ui_down";
	
	
	private Vector2 _velocity = Vector2.Zero;
	private Sprite2D _sprite;

	private float _targetSkew = 0f;

	private Vector2 _direction;
	
	public override void _Ready()
	{
		
		_sprite = GetNode<Sprite2D>("PlayerSprite");
	}

	public override void _Process(double delta)
	{
		var deltaf = (float)delta;
		
		_direction = Input.GetVector(LeftInput, RightInput, UpInput, DownInput);


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
			Rotation = Mathf.Lerp(Rotation, 0f, 10f * deltaf);
		}
		
		// Clamp the rotation so it doesn't exceed the max in either direction
		Rotation = Mathf.Clamp(Rotation, -MaxRotation, MaxRotation);
		
		_skew(deltaf);
		_squash();
		
		_move(deltaf);
	}

	private void _move(float deltaf)
	{
		_velocity = 
			_direction != Vector2.Zero ? 
				_velocity.MoveToward(_direction * MaxSpeed, Acceleration * deltaf) :
				_velocity.MoveToward(Vector2.Zero, Deceleration * deltaf);
		
		Position += _velocity * deltaf;
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
		x.Y = Mathf.Lerp(x.Y, _targetSkew, SkewSpeed * deltaf);
		
		// Reassign modified transform
		xform.X = x;
		_sprite.Transform = xform;
	}
}

using Godot;
using System;

public partial class Player : Node2D
{

	[Export] public float MaxSpeed = 250f;
	[Export] public float Acceleration = 500f;
	[Export] public float Deceleration = 1000f;
	
	[Export] public float RotationSpeed = 1f;
	[Export] public float MaxRotation =  0.2f;
	[Export] public StringName LeftInput = "ui_left";
	[Export] public StringName RightInput = "ui_right";
	[Export] public StringName UpInput = "ui_up";
	[Export] public StringName DownInput = "ui_down";
	
	
	private Vector2 velocity = Vector2.Zero;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		var deltaf = (float)delta;
		
		var direction = Input.GetVector(LeftInput, RightInput, UpInput, DownInput);


		// going right
		if (direction.X < 0)
		{
			Rotation -= RotationSpeed * deltaf;
		}
		// going left
		else if (direction.X > 0)
		{
			Rotation += RotationSpeed * deltaf;
		}    
		else
		{
			// Smoothly rotate back to neutral when not pressing left/right
			Rotation = Mathf.Lerp(Rotation, 0f, 10f * deltaf);
		}

		velocity = 
			direction != Vector2.Zero ? 
			velocity.MoveToward(direction * MaxSpeed, Acceleration * deltaf) :
			velocity.MoveToward(Vector2.Zero, Deceleration * deltaf);
		
		Position += velocity * deltaf;
		
		
		// Clamp the rotation so it doesn't exceed the max in either direction
		Rotation = Mathf.Clamp(Rotation, -MaxRotation, MaxRotation);
	}
}

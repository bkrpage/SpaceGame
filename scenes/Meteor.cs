using Godot;
using System;

public partial class Meteor : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Setup event/signal handling.
		BodyEntered += OnBodyEntered;

	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print("entered " + body);
	}
}

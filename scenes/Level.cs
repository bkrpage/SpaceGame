using Godot;
using System;

public partial class Level : Node2D
{
	[Export] public double MeteorTimerTimout = 1.0;
	
	private PackedScene _meteorScene = GD.Load<PackedScene>("res://scenes/meteor.tscn");
	
	private Node2D _meteorNode;
	private Timer _meteorTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_meteorNode = GetNode<Node2D>("Meteors");
		// Register Timer Signal
		_meteorTimer =  GetNode<Timer>("MeteorTimer");
		_meteorTimer.WaitTime = MeteorTimerTimout;
		_meteorTimer.Start();
		_meteorTimer.Timeout += _onMeteorTimerTimeout;
		
	}

	private void _onMeteorTimerTimeout()
	{
		var meteor = _meteorScene.Instantiate();
		// If we want to make a series of 'special' meteors, we can do that using the below casting.
		// ((Meteor)meteor).BaseSpeed = 5000f;
		_meteorNode.AddChild(meteor);
	}
}

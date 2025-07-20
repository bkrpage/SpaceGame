using Godot;
using System;

public partial class Level : Node2D
{
	
	private Timer _meteorTimer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Register Timer Signal
		_meteorTimer =  GetNode<Timer>("MeteorTimer");
		_meteorTimer.Timeout += _onMeteorTimerTimeout;
	}

	private static void _onMeteorTimerTimeout()
	{
		GD.Print("Timer Timeout");
	}

	
	
}

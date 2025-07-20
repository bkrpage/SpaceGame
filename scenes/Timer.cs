using Godot;
using System;

public partial class Timer : Godot.Timer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		// Setup event/signal handling.
		Timeout += _OnTimeout;
	}
	
	private static void _OnTimeout()
	{
		GD.Print("Timer Timeout");
	}
}

using Godot;
using System;

public partial class Level : Node2D
{
	[Export] public double MeteorTimerTimout = 1.0;
	
	private PackedScene _meteorScene = GD.Load<PackedScene>("res://scenes/meteor.tscn");
	private PackedScene _laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
	
	private Player _playerNode;
	
	private Node2D _lasersNode;

	private Node2D _meteorsNode;
	private Timer _meteorTimerNode;

	public override void _Ready()
	{
		_getNodes();
		
		_registerMeteorTimer();
		_registerShootLaser();
		
	}


	/// <summary>
	/// Get the nodes used within the level and assign to the instance variables.
	/// </summary>
	private void _getNodes()
	{
		_meteorsNode = GetNode<Node2D>("Meteors");
		_playerNode = GetNode<Player>("Player");
		_meteorsNode = GetNode<Node2D>("Meteors");
		_meteorTimerNode =  GetNode<Timer>("MeteorTimer");
		_lasersNode =  GetNode<Node2D>("Lasers");
	}

	private void _registerShootLaser()
	{
		_playerNode.ShootLaser += _onShootLaser;
	}

	private void _registerMeteorTimer()
	{
		// Register Timer Signal
		_meteorTimerNode.WaitTime = MeteorTimerTimout;
		_meteorTimerNode.Start();
		_meteorTimerNode.Timeout += _onMeteorTimerTimeout;	
	}
	
	private void _onShootLaser(Vector2 position)
	{
		var laser =  _laserScene.Instantiate() as Laser;
		laser.Position = position;
		_lasersNode.AddChild(laser);
	}

	private void _onMeteorTimerTimeout()
	{
		var meteor = _meteorScene.Instantiate() as Meteor;
		// If we want to make a series of 'special' meteors, we can do that using the below:
		// meteor.BaseSpeed = 5000f;
		_meteorsNode.AddChild(meteor);
	}
}

using Godot;
using System;

public partial class FloatingText : Marker2D
{

	[Export] public float MaxRotation = 20f;

	
	// Nodes
	private GameState _gameStateNode;

	public override void _Ready()
	{
		_getNodes();
		_randomiseRotation();
	}

	private void _randomiseRotation()
	{
		RotationDegrees = GameState.Rng.RandfRange(-MaxRotation, MaxRotation);
	}

	private void _getNodes()
	{
		_gameStateNode = GetNode<GameState>("/root/GameState");
	}

	public void SetText(string text)
	{
		// TODO refactor this to be more safe to changes.
		GetChild<CenterContainer>(0).GetChild<Label>(0).Text = text;
	}
}

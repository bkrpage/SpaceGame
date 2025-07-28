using Godot;
using System;

public partial class FloatingText : Marker2D
{

	[Export] public float MaxRotation = 20f;

	
	// Nodes
	private Global _globalNode;

	public override void _Ready()
	{
		_getNodes();
		_randomiseRotation();
	}

	private void _randomiseRotation()
	{
		RotationDegrees = _globalNode.Rng.RandfRange(-MaxRotation, MaxRotation);
	}

	private void _getNodes()
	{
		_globalNode = GetNode<Global>("/root/Global");
	}

	public void SetText(string text)
	{
		// TODO refactor this to be more safe to changes.
		GetChild<CenterContainer>(0).GetChild<Label>(0).Text = text;
	}
}

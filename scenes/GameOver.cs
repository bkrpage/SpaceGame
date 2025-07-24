using Godot;
using System;

public partial class GameOver : CanvasLayer
{

	[Export] public PackedScene LevelScene = GD.Load<PackedScene>("res://scenes/level.tscn");

	private Global _globalNode;
	private Label _scoreNode;

	public override void _Ready()
	{
		_getNodes();
		
		_scoreNode.Text += $" {_globalNode.Score}";
	}
	public override void _Process(double delta)
	{
		_handleInput();
	}
	
	private void _getNodes()
	{
		_globalNode = GetNode<Global>("/root/Global");
		_scoreNode =  GetNode<Label>("LargeContainer/GameOverScoreContainer/Score");
	}
	private void _handleInput()
	{
		if (Input.IsActionJustPressed("shoot"))
		{
			GetTree().ChangeSceneToPacked(LevelScene);
		}
	}
}

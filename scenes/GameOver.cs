using Godot;
using System;

public partial class GameOver : CanvasLayer
{

	[Export] public PackedScene LevelScene = GD.Load<PackedScene>("res://scenes/level.tscn");

	private GameState _gameStateNode;
	private Label _scoreNode;
	
	private AudioStreamPlayer2D _deathSoundStreamPlayerNode;

	public override void _Ready()
	{
		_getNodes();
		_playDeathSound();
		
		_scoreNode.Text += $" {_gameStateNode.Score}";
	}
	public override void _Process(double delta)
	{
		_handleInput();
	}
	
	private void _getNodes()
	{
		_gameStateNode = GetNode<GameState>("/root/GameState");
		_scoreNode =  GetNode<Label>("LargeContainer/GameOverScoreContainer/Score");
		_deathSoundStreamPlayerNode = GetNode<AudioStreamPlayer2D>("DeathSound");
	}
	private void _handleInput()
	{
		if (Input.IsActionJustPressed("shoot"))
		{
			GetTree().ChangeSceneToPacked(LevelScene);
		}
	}
	private void _playDeathSound()
	{
		_deathSoundStreamPlayerNode.Play();
	}
}

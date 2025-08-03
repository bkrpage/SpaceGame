using Godot;
using System;

public partial class Ui : CanvasLayer
{

	[Export] public Texture2D LifeTexture = GD.Load<Texture2D>("res://assets/graphic/UI/playerLife1_orange.png");

	
	private GameState _gameStateNode;
	private HBoxContainer _livesContainerNode;
	private Label _scoreLabelNode;
	
	
	public override void _Ready()
	{
		_getNodes();
		_registerScoreSignal();
	}

	public override void _ExitTree()
	{
		_cleanupSignals();
		base._ExitTree();
	}

	private void _getNodes()
	{
		_gameStateNode = GetNode<GameState>("/root/GameState");
		_livesContainerNode =  GetNode<HBoxContainer>("LivesOuterContainer/LivesContainer");
		_scoreLabelNode =  GetNode<Label>("MarginContainer/Label");
	}

	private void _cleanupSignals()
	{
		_gameStateNode.ScoreUpdate -= _updateScore;
	}
	
	private void _registerScoreSignal()
	{
		_gameStateNode.ScoreUpdate += _updateScore;	
	}

	private void _updateScore(int score)
	{
		_scoreLabelNode.Text = $"Score: {score}";
	}
	

	private void SetHealth(int amount){
		foreach (var life in _livesContainerNode.GetChildren())
		{
			life.QueueFree();
		}

		for (int i = 0; i < amount; i++)
		{
			var life = new  TextureRect();
			life.Texture = LifeTexture;
			life.StretchMode = TextureRect.StretchModeEnum.Keep;
			_livesContainerNode.AddChild(life);
			
		}
	}
}

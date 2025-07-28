using Godot;
using System;

public partial class Ui : CanvasLayer
{

	[Export] public Texture2D LifeTexture = GD.Load<Texture2D>("res://assets/graphic/UI/playerLife1_orange.png");

	[Export] public double ScoreTimerTimeout = 0.5;
	
	private Global _globalNode;
	private HBoxContainer _livesContainerNode;
	private Timer _scoreTimerNode;
	private Label _scoreLabelNode;
	
	
	public override void _Ready()
	{
		_getNodes();
		_registerScoreTimer();
		_registerScoreSignal();
	}

	public override void _ExitTree()
	{
		_cleanupSignals();
		base._ExitTree();
	}

	private void _getNodes()
	{
		_globalNode = GetNode<Global>("/root/Global");
		_scoreTimerNode =  GetNode<Timer>("ScoreTimer");
		_livesContainerNode =  GetNode<HBoxContainer>("LivesOuterContainer/LivesContainer");
		_scoreLabelNode =  GetNode<Label>("MarginContainer/Label");
	}

	private void _cleanupSignals()
	{
		_scoreTimerNode.Timeout -= _onScoreTimerTimeout;
		_globalNode.ScoreUpdate -= _updateScore;
	}
	
	private void _registerScoreTimer()
	{
		_scoreTimerNode.WaitTime = ScoreTimerTimeout;
		_scoreTimerNode.Start();
		_scoreTimerNode.Timeout += _onScoreTimerTimeout;
	}
	
	private void _registerScoreSignal()
	{
		_globalNode.ScoreUpdate += _updateScore;	
	}

	private void _updateScore(int score)
	{
		_scoreLabelNode.Text = $"Score: {score}";
	}

	private void _onScoreTimerTimeout()
	{
		_globalNode.IncrementScore();
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

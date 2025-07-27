using Godot;
using System;

public partial class Global : Node
{
    public int Score = 0;
    
    [Signal]
    public delegate void ScoreUpdateEventHandler(int score);

    public void UpdateScoreBy(int amount)
    {
        Score += amount;
        EmitSignalScoreUpdate(Score);
    }
    public void IncrementScore()
    {
        Score++;
        EmitSignalScoreUpdate(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        EmitSignalScoreUpdate(Score);
    }
    
    

}

using Godot;
using System;

public partial class Global : Node
{
    public int Score = 0;

    public string Seed;
    public RandomNumberGenerator Rng;
    
    [Signal] public delegate void ScoreUpdateEventHandler(int score);

    public override void _Ready()
    {
        GenerateNewSeed();
    }

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

    public void GenerateNewSeed()
    {
        Seed = Guid.NewGuid().ToString();
        Rng = new RandomNumberGenerator();
        Rng.Seed = (ulong)GD.Hash(Seed);
        
        GD.Print("Run seed: " + Seed);
    }
    
    

}

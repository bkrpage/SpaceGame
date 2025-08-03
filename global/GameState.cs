using Godot;
using System;

public partial class GameState : Node
{
    public int Score = 0;

    public static string Seed { get; private set; }
    public static RandomNumberGenerator Rng { get; private set; }

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

    public static void GenerateNewSeed()
    {
        Seed = Guid.NewGuid().ToString();
        Rng = new RandomNumberGenerator();
        Rng.Seed = (ulong)GD.Hash(Seed);
        
        GD.Print("Run seed: " + Seed);
    }
    
    

}

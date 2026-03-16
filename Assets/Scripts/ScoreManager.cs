using UnityEngine;
using System;
using System.Collections.Generic;

public enum ScoreType
{
    Base,
    Speed,
    LowHealth,
    Air,
    ShieldBypass
}

public class ScoreComponent
{
    public int amount;
    public ScoreType type;
    
    public ScoreComponent(int amount, ScoreType type)
    {
        this.amount = amount;
        this.type = type;
    }
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    private int currentScore = 0;
    public int CurrentScore => currentScore;
    
    public event Action<int> OnScoreChanged;
    public event Action<List<ScoreComponent>> OnScoreAdded;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void AddScore(List<ScoreComponent> components)
    {
        int totalAmount = 0;
        foreach (var component in components)
        {
            totalAmount += component.amount;
        }
        
        currentScore += totalAmount;
        OnScoreChanged?.Invoke(currentScore);
        OnScoreAdded?.Invoke(components);
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
}

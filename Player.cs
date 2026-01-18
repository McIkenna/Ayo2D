using UnityEngine;
using System.Collections.Generic;
using System.Collections;
[System.Serializable]
public class Player
{
   public PlayerType playerType;
    public StoneColor stoneColor;
    public List<Stone> capturedStones = new List<Stone>();
    public List<string> controlledPots = new List<string>(); // Pot IDs
    public int roundsWon = 0; // Track rounds won
    
    public int CapturedStoneCount => capturedStones.Count;
    
    public void CaptureStones(List<Stone> stones)
    {
        capturedStones.AddRange(stones);
    }
}
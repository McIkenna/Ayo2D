using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pot
{
    public string potID; // e.g., "POT1A", "POT2B"
    public List<Stone> stones = new List<Stone>();
    public GameObject potObject;
    public Transform stoneContainer; // Parent transform for organizing stones
    
    public int StoneCount => stones.Count;
    
    public bool IsEmpty => stones.Count == 0;
    
    public void AddStone(Stone stone)
    {
        stones.Add(stone);
        UpdateStoneVisuals();
    }

  
    
    public List<Stone> TakeAllStones()
    {
        List<Stone> takenStones = new List<Stone>(stones);
        stones.Clear();
        UpdateStoneVisuals();
        return takenStones;
    }
    
    public void Clear()
    {
        foreach (var stone in stones)
        {
            stone.DestroyVisual();
        }
        stones.Clear();
    }
    
    public void UpdateStoneVisuals()
{
    if (stoneContainer == null) return;
    
    // Arrange stones in a circular pattern within the pot
    float radius = 0.15f;
    for (int i = 0; i < stones.Count; i++)
    {
        if (stones[i].visualObject != null)
        {
            float angle = 360f / Mathf.Max(stones.Count, 1) * i * Mathf.Deg2Rad;
            
            // For 2D: use x for horizontal, y for vertical (instead of x, y, z)
            Vector2 offset = new Vector2(
                Mathf.Cos(angle) * radius,  // x position (horizontal)
                Mathf.Sin(angle) * radius   // y position (vertical)
            );
            
            // For 2D, you might want to use Vector2 or Vector3 with z=0
            stones[i].visualObject.transform.position = (Vector2)stoneContainer.position + offset;
            // stones[i].visualObject.transform.localScale = new Vector2(0.005f, 0.005f);
            stones[i].visualObject.transform.SetParent(stoneContainer);
        }
    }
}
}
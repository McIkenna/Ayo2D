using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Stone
{
     public StoneColor color;
    public GameObject visualObject;
    
    public Stone(StoneColor stoneColor, GameObject visual = null)
    {
        color = stoneColor;
        visualObject = visual;
    }
    
    public void DestroyVisual()
    {
        if (visualObject != null)
        {
            Object.Destroy(visualObject);
        }
    }
}
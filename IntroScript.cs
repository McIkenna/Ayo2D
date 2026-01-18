using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class IntroScript : MonoBehaviour
{
    public float waitTime = 5.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        StartCoroutine(WaitAndLoadScene());
    }

    IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(1); // Load the scene with index 1 (main game scene)
    }

   
}

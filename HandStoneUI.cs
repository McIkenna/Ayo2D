using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI that follows the player hand and shows stone count
/// </summary>
public class HandStoneUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handTransform; // Player hand object
    [SerializeField] private Canvas worldCanvas;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Transform thumbnailContainer; // Container for multiple thumbnails
    [SerializeField] private GameObject stoneThumbnailPrefab; // Prefab for individual thumbnail
    [SerializeField] private TextMeshProUGUI stoneCountText;
    
    [Header("Stone Thumbnails")]
    [SerializeField] private Sprite blueStoneSprite;
    [SerializeField] private Sprite blackStoneSprite;
    
    [Header("Settings")]
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0); // Offset above hand
    [SerializeField] private float thumbnailSpacing = 0.3f;
    
    private List<GameObject> activeThumbnails = new List<GameObject>();
    
    private void Start()
    {
        HideUI();
    }
    
    private void LateUpdate()
    {
        // Keep UI positioned near hand
        if (handTransform != null && uiPanel.activeSelf)
        {
            transform.position = handTransform.position + uiOffset;
            
            // Make UI face camera
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180, 0); // Flip to face camera
            }
        }
    }
    
    // public void ShowUI(List<Stone> stonesInHand)
    // {
    //     uiPanel.SetActive(true);
    //     UpdateStoneDisplay(stonesInHand);
    // }
    
    // public void UpdateStoneDisplay(List<Stone> stonesInHand)
    // {
    //     // Clear existing thumbnails
    //     ClearThumbnails();
        
    //     if (stonesInHand == null || stonesInHand.Count == 0)
    //     {
    //         HideUI();
    //         return;
    //     }
        
    //     // Update text count
    //     if (stoneCountText != null)
    //     {
    //         stoneCountText.text = stonesInHand.Count.ToString();
    //     }
        
    //     // Create thumbnails for each stone (up to max)
    //     int thumbnailsToCreate = Mathf.Min(stonesInHand.Count, maxThumbnailsToShow);
        
    //     for (int i = 0; i < thumbnailsToCreate; i++)
    //     {
    //         Stone stone = stonesInHand[i];
    //         CreateThumbnail(stone.color, i);
    //     }
        
    //     // If we have more stones than max thumbnails, add "..." indicator
    //     if (stonesInHand.Count > maxThumbnailsToShow)
    //     {
    //         // Optionally show "+X more" text
    //     }
    // }
    
    private void CreateThumbnail(StoneColor color, int index)
    {
        GameObject thumbnail = null;
        
        if (stoneThumbnailPrefab != null && thumbnailContainer != null)
        {
            // Instantiate from prefab
            thumbnail = Instantiate(stoneThumbnailPrefab, thumbnailContainer);
        }
        else if (thumbnailContainer != null)
        {
            // Create simple image if no prefab
            GameObject thumbObj = new GameObject($"Thumbnail_{index}");
            thumbObj.transform.SetParent(thumbnailContainer);
            thumbnail = thumbObj;
            thumbnail.AddComponent<Image>();
        }
        
        if (thumbnail != null)
        {
            // Set sprite based on color
            Image img = thumbnail.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = color == StoneColor.Blue ? blueStoneSprite : blackStoneSprite;
            }
            
            // Position thumbnail
            RectTransform rect = thumbnail.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(index * thumbnailSpacing * 60, 0);
                rect.sizeDelta = new Vector2(50, 50);
            }
            
            activeThumbnails.Add(thumbnail);
        }
    }
    
    private void ClearThumbnails()
    {
        foreach (var thumbnail in activeThumbnails)
        {
            if (thumbnail != null)
            {
                Destroy(thumbnail);
            }
        }
        activeThumbnails.Clear();
    }
    
    public void HideUI()
    {
        uiPanel.SetActive(false);
        ClearThumbnails();
    }
}
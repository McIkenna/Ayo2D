using UnityEngine;
using System.Collections;

public class HandMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handTransform;
    [SerializeField] private SpriteRenderer handSpriteRenderer;
    [SerializeField] private Animator handAnimator;
    
    [Header("Animation Timing")]
    [SerializeField] private float pickUpAnimationDuration = 0.5f;
    [SerializeField] private float dropAnimationDuration = 0.3f;
    
    private Vector2 screenBounds;
    private bool isHoldingStones = false;
    private bool isCurrentlyMoving = false;
    
    // Animation state tracking
    private enum HandState
    {
        Idle,
        Moving,
        PickingUp,
        Dropping
    }
    private HandState currentState = HandState.Idle;

    private void Awake()
    {
        if (handAnimator == null)
        {
            handAnimator = GetComponent<Animator>();
        }
            
    }
    
    private void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        SetState(HandState.Idle);
    }
    
    // Main method to set hand state
    private void SetState(HandState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        UpdateAnimation();
    }
    
    private void UpdateAnimation()
    {
        if (handAnimator == null) return;
        
        // Reset all bools and triggers first to avoid state conflicts
        handAnimator.SetBool("IsMoving", false);
        handAnimator.SetBool("IsIdle", false);
        handAnimator.SetBool("IsHolding", isHoldingStones);
        
        // Set animator parameters based on current state
        switch (currentState)
        {
            case HandState.Idle:
                handAnimator.SetBool("IsIdle", true);
                isCurrentlyMoving = false;
                break;
                
            case HandState.Moving:
                handAnimator.SetBool("IsMoving", true);
                isCurrentlyMoving = true;
                break;
                
            case HandState.PickingUp:
                handAnimator.ResetTrigger("Drop");
                handAnimator.SetTrigger("PickUp");
                isCurrentlyMoving = false;
                break;
                
            case HandState.Dropping:
                handAnimator.ResetTrigger("PickUp");
                handAnimator.SetTrigger("Drop");
                isCurrentlyMoving = false;
                break;
        }
        
        Debug.Log($"Hand State Changed: {currentState} | Holding: {isHoldingStones}");
    }

    // === PUBLIC METHODS TO CALL FROM YOUR COROUTINE ===
    
    /// <summary>
    /// Start the pickup animation. Call this when hand reaches the pot to pick up stones.
    /// </summary>
    public IEnumerator PlayPickUpAnimation()
    {
        isHoldingStones = true;
        SetState(HandState.PickingUp);
        
        // Wait for pickup animation to complete
        yield return new WaitForSeconds(pickUpAnimationDuration);
        SetHoldingStones(true);        
        // After pickup, hand is ready to move
        SetState(HandState.Moving);
    }
    
    /// <summary>
    /// Set hand to moving state. Call this when hand starts traveling to next pot.
    /// </summary>
    public void SetMoving()
    {
        SetState(HandState.Moving);
    }
    
    /// <summary>
    /// Play the drop animation. Call this when placing a stone in a pot.
    /// </summary>
    public IEnumerator PlayDropAnimation(bool hasMoreStones)
    {
        SetState(HandState.Dropping);
        
        // Wait for drop animation
        yield return new WaitForSeconds(dropAnimationDuration);
        
        // After dropping, ALWAYS go to moving state (don't go idle yet)
        // Only SetIdle() should make it idle (called at end of turn)
        if (hasMoreStones)
        {
            isHoldingStones = true;
        }
        else
        {
            isHoldingStones = false;
        }
        
        // Stay in moving/holding state, don't auto-idle
        SetState(HandState.Moving);
    }
    
    /// <summary>
    /// Set whether hand is holding stones (updates animation)
    /// </summary>
    public void SetHoldingStones(bool holding)
    {
        isHoldingStones = holding;
        
        // Update animation to reflect holding state
        if (handAnimator != null)
        {
            handAnimator.SetBool("IsHolding", isHoldingStones);
        }
    }
    
    /// <summary>
    /// Return hand to idle state. Call when turn completely ends.
    /// </summary>
    public void SetIdle()
    {
        isHoldingStones = false;
        SetState(HandState.Idle);
    }
    
    /// <summary>
    /// Check if hand is currently in moving state
    /// </summary>
    public bool IsMoving()
    {
        return isCurrentlyMoving;
    }
    
    /// <summary>
    /// Check if hand is holding stones
    /// </summary>
    public bool IsHoldingStones()
    {
        return isHoldingStones;
    }
    
    /// <summary>
    /// Get current hand state (for debugging)
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }
    
    // Optional: Change hand sprite appearance
    public void SetHandAppearance(Sprite newSprite)
    {
        if (handSpriteRenderer != null)
        {
            handSpriteRenderer.sprite = newSprite;
        }
    }
}
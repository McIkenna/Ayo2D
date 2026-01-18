using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
public class AyoPotTrigger : MonoBehaviour
{
    [Header("Pot Configuration")]
    [SerializeField] private string potID; // e.g., "POT1A"

    [Header("References")]
    [SerializeField] private AyoGameManager gameManager;
    [SerializeField] private Renderer potRenderer; // For changing color on hover
    [SerializeField] private Camera mainCamera;

    [Header("Hover Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0.4f);
    [SerializeField] private float highlightIntensity = 2f;
    [SerializeField] private float highlightDuration = 0.8f;

    [Header("Computer Selection")]
    [SerializeField] private Color computerSelectedColor = new Color(0.8f, 0.35f, 0.35f, 0.7f);
    [SerializeField] private float computerGlowDuration = 1.3f;
    [SerializeField] private float computerGlowIntensity = 0.5f;

    private Coroutine computerGlowCoroutine;

    [Header("Mobile Performance")]
    // [SerializeField] private float mobileUpdateInterval = 0.05f;
    [SerializeField] private bool useLightweightHighlight = true;

    [Header("Touch Settings")]
    [SerializeField] private bool enableTouchHover = false; // Optional: disable hover on touch


    // Performance optimization
    // private float lastUpdateTime = 0f;
    private Coroutine highlightCoroutine;
    private Material potMaterial;
    private Color originalColor;
    private Color originalEmissionColor;
    private bool isHovered = false;
    private PlayerInput playerInput;
    private Ray ray;
    private RaycastHit hit;

    // Touch tracking
    private bool isTouching = false;
    private float touchStartTime = 0f;
    private Vector2 touchStartPosition;
    bool potSelected = false;
    private void Awake()
    {
        // Get camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Get renderer if not assigned - searches children automatically
        if (potRenderer == null)
        {
            potRenderer = GetComponentInChildren<Renderer>();
        }

        // Store original material and color
        if (potRenderer != null)
        {
            potMaterial = potRenderer.material;
            originalColor = potMaterial.color;
            OptimizeMaterialForMobile();
        }
        else
        {
            Debug.LogWarning($"{potID}: No Renderer found! Color highlighting won't work.");
        }

        // Initialize Input System
        playerInput = new PlayerInput();
    }

    private void OptimizeMaterialForMobile()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Use mobile-optimized shader if available
        if (potMaterial != null)
        {
            // Disable emission (expensive on mobile)
            potMaterial.DisableKeyword("_EMISSION");
            
            // Disable unnecessary features
            potRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            potRenderer.receiveShadows = false;
            
            Debug.Log($"{potID}: Material optimized for mobile");
        }
#endif
    }
    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Click.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        playerInput.Player.Click.performed -= OnClickPerformed;
        playerInput.Player.Disable();
    }

    private void Update()
    {
        CheckHover();
        HandleTouchInput();
    }

    IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(2f); // wait 3 seconds

        ResetColor();
        // Your request logic here
    }
    private void CheckHover()
    {
        if (mainCamera == null) return;

        if (IsPointerOverUI())
        {
            if (isHovered)
            {
                OnHoverExit();
            }
            return;
        }

        // Get mouse/touch position
        Vector2 inputPosition = Vector2.zero;
        // Check for mouse
        bool hasInput = false;
        if (Mouse.current != null)
        {
            inputPosition = Mouse.current.position.ReadValue();
            hasInput = true;
        }
        // Touch hover (optional)
        if (Touchscreen.current != null && enableTouchHover)
        {
            if (Touchscreen.current.primaryTouch.press.isPressed)
            {
                inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                hasInput = true;
            }
        }
        // Check for touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        //else
        // {
        // // No input, reset hover
        //if (isHovered)
        //{
        //   OnHoverExit();
        // }
        // return;
        //}
        if (!hasInput)
        {
            if (isHovered) OnHoverExit();
            return;
        }

        // Raycast from camera through input position
        ray = mainCamera.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out hit))
        {
            // Check if ray hit this pot
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (!isHovered)
                {
                    OnHoverEnter();
                }

            }
            else
            {
                if (isHovered)
                {
                    OnHoverExit();
                }
            }
        }
        else
        {
            if (isHovered)
            {
                OnHoverExit();
            }
        }
    }


    private void HandleTouchInput()
    {
        // Handle direct touch input for mobile
        if (potSelected)
        {
            StartCoroutine(DelayedReset());
            potSelected = false;

        }

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            // Touch began
            if (touch.press.wasPressedThisFrame)
            {
                isTouching = true;
                touchStartTime = Time.time;
                touchStartPosition = touch.position.ReadValue();
                int fingerId = touch.touchId.ReadValue();
                // Don't process if over UI
                if (IsPointerOverUITouch(touchStartPosition))
                {
                    isTouching = false;
                    return;
                }

                Debug.Log($"Touch began at {touchStartPosition}");
            }

            // Touch ended
            if (touch.press.wasReleasedThisFrame && isTouching)
            {
                float touchDuration = Time.time - touchStartTime;
                Vector2 touchEndPosition = touch.position.ReadValue();
                float touchDistance = Vector2.Distance(touchStartPosition, touchEndPosition);

                Debug.Log($"Touch ended. Duration: {touchDuration}, Distance: {touchDistance}");

                // Check if it's a tap (short duration, minimal movement)
                if (touchDuration < 0.5f && touchDistance < 50f)
                {
                    ProcessTouchTap(touchEndPosition);
                }

                isTouching = false;


            }

            // Touch cancelled
            if (!touch.press.isPressed)
            {
                isTouching = false;
            }
        }
    }
    private void ProcessTouchTap(Vector2 touchPosition)
    {
        if (mainCamera == null) return;
        if (Touchscreen.current == null) return;
        if (gameManager == null) return;

        // Don't process if over UI
        if (IsPointerOverUITouch(touchPosition))
        {
            Debug.Log("Touch was over UI, ignoring");
            return;
        }

        // Raycast from touch position
        Ray touchRay = mainCamera.ScreenPointToRay(touchPosition);
        RaycastHit touchHit;

        if (Physics.Raycast(touchRay, out touchHit))
        {
            Debug.Log($"Touch hit: {touchHit.collider.gameObject.name}");

            // Check if touch hit this pot
            if (touchHit.collider.gameObject == gameObject || touchHit.collider.transform.IsChildOf(transform))
            {
                // Check if this pot can be selected
                if (gameManager.CanSelectPot(potID))
                {
                    Debug.Log($"Pot {potID} tapped on mobile!");

                    SetPotColor(selectedColor);
                    potSelected = true;

                    // Notify game manager
                    gameManager.OnPotSelected(potID);
                }
                else
                {
                    Debug.Log($"Pot {potID} cannot be selected");
                }
            }
        }
        else
        {
            Debug.Log("Touch didn't hit anything");
        }
    }
    private void OnHoverEnter()
    {
        if (gameManager == null) return;

        // Check if this pot can be selected
        if (gameManager.CanSelectPot(potID))
        {
            isHovered = true;
            SetPotColor(hoverColor);
            Debug.Log($"Hovering over {potID}");
        }
    }

    private void OnHoverExit()
    {
        if (isHovered)
        {
            ResetColor();
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // Ignore touch if it is REALLY pressing
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
            return;


        if (IsPointerOverUI())
        {
            return; // Don't select pot if clicking on UI
        }

        if (!isHovered) return;
        if (gameManager == null) return;

        // Check if this pot can be selected
        if (gameManager.CanSelectPot(potID))
        {
            Debug.Log($"Pot {potID} clicked!");
            SetPotColor(selectedColor);
            potSelected = true;

            // Notify game manager
            gameManager.OnPotSelected(potID);
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (Mouse.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        // if (EventSystem.current.IsPointerOverGameObject()) return;

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("BlockingUI"))
            {
                return true;
            }
        }

        return false;
    }


    // Check if touch position is over any UI element
    private bool IsPointerOverUITouch(Vector2 touchPosition)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = touchPosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);


        foreach (var result in results)
        {
            // Block if over any UI element on touch
            if (result.gameObject.CompareTag("BlockingUI"))
            {
                Debug.Log($"Touch over UI: {result.gameObject.name}");
                return true;
            }
        }

        return false;
    }
    private void SetPotColor(Color color)
    {
        if (potMaterial != null)
        {
            potMaterial.color = color;
        }
    }

    // Reset color to normal
    public void ResetColor()
    {
        SetPotColor(normalColor);
        isHovered = false;
    }

    // OPTIMIZED: Stone drop highlight effect for mobile
    public void TriggerStoneDropHighlight()
    {
        // Stop any existing highlight
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        if (useLightweightHighlight)
        {
            highlightCoroutine = StartCoroutine(LightweightHighlightCoroutine());
        }
        else
        {
            highlightCoroutine = StartCoroutine(StandardHighlightCoroutine());
        }
    }

    // MOBILE OPTIMIZED: Simple color flash (no emission)
    private IEnumerator LightweightHighlightCoroutine()
    {
        if (potMaterial == null) yield break;

        // Create bright highlight color
        Color brightColor = highlightColor * highlightIntensity;
        brightColor.a = 1f;

        float halfDuration = highlightDuration / 1.5f;
        float elapsed = 0f;

        // Fade to bright
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            potMaterial.color = Color.Lerp(originalColor, brightColor, t);
            yield return null;
        }

        // Fade back
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            potMaterial.color = Color.Lerp(brightColor, originalColor, t);
            yield return null;
        }

        // Ensure original color
        potMaterial.color = originalColor;
        highlightCoroutine = null;
    }

    // DESKTOP: Full emission effect (if supported)
    private IEnumerator StandardHighlightCoroutine()
    {
        if (potMaterial == null) yield break;

        // Try to use emission if supported
        bool supportsEmission = potMaterial.HasProperty("_EmissionColor");
        Color originalEmission = Color.black;

        if (supportsEmission)
        {
            potMaterial.EnableKeyword("_EMISSION");
            originalEmission = potMaterial.GetColor("_EmissionColor");
        }

        Color targetEmission = highlightColor * highlightIntensity;
        float halfDuration = highlightDuration / 1.5f;
        float elapsed = 0f;

        // Fade in
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            if (supportsEmission)
            {
                Color currentEmission = Color.Lerp(originalEmission, targetEmission, t);
                potMaterial.SetColor("_EmissionColor", currentEmission);
            }
            else
            {
                // Fallback to color change
                Color brightColor = Color.Lerp(originalColor, highlightColor * 2f, t);
                potMaterial.color = brightColor;
            }

            yield return null;
        }

        // Fade out
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            if (supportsEmission)
            {
                Color currentEmission = Color.Lerp(targetEmission, originalEmission, t);
                potMaterial.SetColor("_EmissionColor", currentEmission);
            }
            else
            {
                Color brightColor = Color.Lerp(highlightColor * 2f, originalColor, t);
                potMaterial.color = brightColor;
            }

            yield return null;
        }

        // Reset
        if (supportsEmission)
        {
            potMaterial.SetColor("_EmissionColor", originalEmission);
        }
        potMaterial.color = originalColor;

        highlightCoroutine = null;
    }


    public void TriggerComputerSelection()
    {
        // Stop any existing glow
        if (computerGlowCoroutine != null)
        {
            StopCoroutine(computerGlowCoroutine);
        }

        computerGlowCoroutine = StartCoroutine(ComputerSelectionGlowCoroutine());
    }

    // Coroutine for computer selection glow effect
    private IEnumerator ComputerSelectionGlowCoroutine()
    {
        if (potMaterial == null) yield break;

        // Store original color
        Color originalPotColor = potMaterial.color;

        // Create bright red glow color
        Color redGlow = computerSelectedColor * computerGlowIntensity;
        redGlow.a = 1f;

        float halfDuration = computerGlowDuration / 2f;
        float elapsed = 0f;

        // Fade to red glow
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            potMaterial.color = Color.Lerp(originalPotColor, redGlow, t);
            yield return null;
        }

        // Hold the glow briefly
        yield return new WaitForSeconds(0.3f);

        // Fade back to original
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            potMaterial.color = Color.Lerp(redGlow, originalPotColor, t);
            yield return null;
        }

        // Ensure original color
        potMaterial.color = originalPotColor;
        computerGlowCoroutine = null;
    }

    private void OnDestroy()
    {
        // Clean up
        if (playerInput != null)
        {
            playerInput.Dispose();
        }
    }
}
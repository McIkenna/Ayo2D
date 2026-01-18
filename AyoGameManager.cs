using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AyoGameManager : MonoBehaviour
{
    [Header("Stone Prefabs")]
    [SerializeField] private GameObject blueStonePrefab;
    [SerializeField] private GameObject blackStonePrefab;
    [SerializeField] private Canvas backgroundCanvas;

    [Header("UI References")]
    [SerializeField] private AyoUIManager uiManager;

    [Header("Hand References")]
    [SerializeField] private Transform playerHandTransform; // The player's hand object
    [SerializeField] private Transform computerHandTransform; // The computer's hand object
    [SerializeField] private float handMoveSpeed = 5f;
    [SerializeField] private HandMovement handMovement;
    [SerializeField] private AIMovement aiMovement;

    [Header("Game Settings")]
    [SerializeField] private int initialStonesPerPot = 4;
    [SerializeField] private float computerThinkTime = 1.5f;
    [SerializeField] private float stoneDropDelay = 0.2f;
    [SerializeField] private float stonePickupDelay = 0.3f;
    [SerializeField] private int maxRounds = 5;
    [SerializeField] private Difficulty aiDifficulty = Difficulty.Medium;
    [SerializeField] private float roundResultDisplayTime = 4f; // Time to show round result
    [SerializeField] private float roundStartDisplayTime = 3f; // Time to show round start

    [Header("Pot References")]
    [SerializeField] private List<Pot> allPots = new List<Pot>();

    [Header("Player Data")]
    private Player player1;
    private Player computer;

    [Header("Visual Settings")]
    [SerializeField] private Transform capturedStonesPlayer1Container;
    [SerializeField] private Transform capturedStonesComputerContainer;

    private GameState currentState;
    private Player currentPlayer;
    private Pot selectedPot;
    private List<Stone> stonesInHand = new List<Stone>();
    private int currentPotIndex = 0;
    // private Vector2 originalHandPosition;
    // private Vector2 computerOriginalHandPosition;
    private int currentRound = 1;
    private Player lastPlayerToCapture = null;

    
    // Store positions differently based on object type
    private Vector3 originalHandLocalPosition;
    private Vector3 computerOriginalHandLocalPosition;
    private Vector2 originalHandAnchoredPosition;
    private Vector2 computerOriginalAnchoredPosition;
    
    // Track if hands are UI elements (RectTransform) or world objects (Transform)
    private bool playerHandIsUI;
    private bool computerHandIsUI;
    
    private RectTransform playerHandRect;
    private RectTransform computerHandRect;


    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBackgroundMusic();
        }
        InitializeHandPositions();

        InitializeGame();
    }

    private void InitializeHandPositions()
    {
        // Player hand
        if (playerHandTransform != null)
        {
            playerHandRect = playerHandTransform.GetComponent<RectTransform>();
            
            if (playerHandRect != null)
            {
                // It's a UI element - store anchored position
                playerHandIsUI = true;
                originalHandAnchoredPosition = playerHandRect.anchoredPosition;
                Debug.Log($"Player hand (UI) original anchored position: {originalHandAnchoredPosition}");
            }
            else
            {
                // It's a world object - store local position
                playerHandIsUI = false;
                originalHandLocalPosition = playerHandTransform.localPosition;
                Debug.Log($"Player hand (World) original local position: {originalHandLocalPosition}");
            }
        }
        
        // Computer hand
        if (computerHandTransform != null)
        {
            computerHandRect = computerHandTransform.GetComponent<RectTransform>();
            
            if (computerHandRect != null)
            {
                // It's a UI element - store anchored position
                computerHandIsUI = true;
                computerOriginalAnchoredPosition = computerHandRect.anchoredPosition;
                Debug.Log($"Computer hand (UI) original anchored position: {computerOriginalAnchoredPosition}");
            }
            else
            {
                // It's a world object - store local position
                computerHandIsUI = false;
                computerOriginalHandLocalPosition = computerHandTransform.localPosition;
                Debug.Log($"Computer hand (World) original local position: {computerOriginalHandLocalPosition}");
            }
        }
    }

    // ========== INITIALIZATION ==========
    private void InitializeGame()
    {
        // Initialize players
        computer = new Player { playerType = PlayerType.Computer, stoneColor = StoneColor.Blue };
        player1 = new Player { playerType = PlayerType.Player1, stoneColor = StoneColor.Black };

        // Setup pots
        SetupPots();

        // Initialize stones in pots
        InitializeStones();

        // Player 1 starts
        currentPlayer = player1;
        currentState = GameState.WaitingForPlayerSelection;

        Debug.Log("Game Initialized! Player 1's turn.");
        currentRound = 1;

        // Show round start notification
        if (uiManager != null)
        {
            uiManager.ShowRoundStart(currentRound);
        }

        // Wait for notification before starting gameplay
        StartCoroutine(DelayedGameStart());

        Debug.Log("Game Initialized! Round 1 starting...");
    }

    private IEnumerator DelayedGameStart()
    {
        // Wait for round start notification
        yield return new WaitForSeconds(roundStartDisplayTime);

        currentState = GameState.WaitingForPlayerSelection;
        Debug.Log("Player 1's turn!");
    }

    private void SetupPots()
    {
        // Assign controlled pots
        computer.controlledPots.AddRange(new[] { "pot1hole", "pot2hole", "pot3hole", "pot4hole", "pot5hole", "pot6hole" });
        player1.controlledPots.AddRange(new[] { "pot7hole", "pot8hole", "pot9hole", "pot10hole", "pot11hole", "pot12hole" });
    }

    private void InitializeStones()
    {
        foreach (var pot in allPots)
        {
            pot.Clear();

            int potNumber = int.Parse(
                    System.Text.RegularExpressions.Regex.Match(pot.potID, @"\d+").Value
                );
            StoneColor color = potNumber <= 6
                ? StoneColor.Blue
                : StoneColor.Black;

            GameObject stonePrefab = color == StoneColor.Blue
                ? blueStonePrefab
                : blackStonePrefab;

            // Add initial stones
            for (int i = 0; i < initialStonesPerPot; i++)
            {
                // Instantiate visual stone
                GameObject stoneVisual = Instantiate(stonePrefab, pot.stoneContainer);
                stoneVisual.transform.localPosition = new Vector3(0, 0, 0);
                stoneVisual.name = $"{color}Stone_{i}";

                // Create stone data
                Stone stone = new Stone(color, stoneVisual);
                pot.AddStone(stone);
            }

            // Update visual positions
            pot.UpdateStoneVisuals();
        }
    }

    // ========== POT SELECTION ==========
    public bool CanSelectPot(string potID)
    {
        // Can only select during player's turn
        if (currentState != GameState.WaitingForPlayerSelection) return false;
        if (currentPlayer.playerType != PlayerType.Player1) return false;

        // Check if player controls this pot
        if (!currentPlayer.controlledPots.Contains(potID)) return false;

        // Check if pot is not empty
        Pot pot = GetPotByID(potID);
        if (pot == null || pot.IsEmpty) return false;

        return true;
    }

    public void OnPotSelected(string potID)
    {
        if (!CanSelectPot(potID)) return;
        if (currentState != GameState.WaitingForPlayerSelection) return;
        if (currentPlayer.playerType != PlayerType.Player1) return;

        Pot pot = GetPotByID(potID);
        if (pot == null || pot.IsEmpty || !currentPlayer.controlledPots.Contains(potID)) return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPotSelect();
        }

        Debug.Log($"Player selected {potID}");
        selectedPot = pot;
        StartMove();
    }

// ========== DIFFICULTY MANAGEMENT ==========
    public void SetDifficulty(Difficulty newDifficulty)
    {
        aiDifficulty = newDifficulty;
        Debug.Log($"AI Difficulty set to: {aiDifficulty}");
    }

    public Difficulty GetDifficulty()
    {
        return aiDifficulty;
    }
    // ========== MOVE EXECUTION ==========
    private void StartMove()
    {
        currentState = GameState.PlayerMoving;
        StartCoroutine(StartMoveSequence());
    }

    private IEnumerator StartMoveSequence()
    {
        Transform activeHand = currentPlayer == player1 ? playerHandTransform : computerHandTransform;
        // Hide stone visuals (they're now "in hand")
        Debug.Log($"activeHand {activeHand} ...");
        if (activeHand != null)
        {

            yield return StartCoroutine(MoveHandToPot(selectedPot, activeHand));

            if (currentPlayer == player1 && SoundManager.Instance != null)
            {

                SoundManager.Instance.PlayStonePickup();
                yield return StartCoroutine(handMovement.PlayPickUpAnimation());
            }
            if (currentPlayer == computer && SoundManager.Instance != null)
            {
                 SoundManager.Instance.PlayStonePickup();
                yield return StartCoroutine(aiMovement.PlayPickUpAnimation());
            }
        {
            
        }
        }


        // Update holding state
        // handMovement.SetHoldingStones(true);

        stonesInHand = selectedPot.TakeAllStones();

        // Play stone pickup sound
        

        Debug.Log($"Picked up {stonesInHand.Count} stones from {selectedPot.potID}");
        if (uiManager != null)
                {
                    uiManager.ShowStonesInHand(stonesInHand);
                }
        // Hide stone visuals
        foreach (var stone in stonesInHand)
        {
            if (stone.visualObject != null)
            {
                // SoundManager.Instance.PlayStonePickup();
                stone.visualObject.SetActive(false);
            }
        }

        // Show UI Manager stones in hand
        

        // Find starting pot index
        currentPotIndex = allPots.IndexOf(selectedPot);
        // yield return new WaitForSeconds(stonePickupDelay);
        // Start distributing stones
        StartCoroutine(DistributeStones());
    }

    private StoneColor GetDominantStoneColor(List<Stone> stones)
    {
        int blueCount = 0;
        int blackCount = 0;

        foreach (var stone in stones)
        {
            if (stone.color == StoneColor.Blue) blueCount++;
            else blackCount++;
        }

        return blueCount >= blackCount ? StoneColor.Blue : StoneColor.Black;
    }

    private IEnumerator DistributeStones()
    {
        // Get the appropriate hand transform based on current player
        Transform activeHand = currentPlayer == player1 ? playerHandTransform : computerHandTransform;

        Debug.Log($"activeHand {activeHand} ...");

        // Move hand to the selected pot
        yield return StartCoroutine(MoveHandToPot(allPots[currentPotIndex], activeHand));


        while (stonesInHand.Count > 0)
        {
            // Move to next pot CLOCKWISE (decrement for clockwise movement)
            if (uiManager != null)
            {
                uiManager.UpdateStonesInHandDisplay(stonesInHand);
            }
            currentPotIndex = (currentPotIndex + 1) % allPots.Count;
            Pot currentPot = allPots[currentPotIndex];

            // Move hand to pot position
            if (activeHand != null)
            {
                yield return StartCoroutine(MoveHandToPot(currentPot, activeHand));
            }

            // Take one stone from hand
            Stone stone = stonesInHand[0];
            stonesInHand.RemoveAt(0);

            bool isLastStone = stonesInHand.Count == 0;
            bool potWasEmpty = currentPot.IsEmpty;

            Debug.Log($"current pot empty {currentPot.IsEmpty}");
            // // Update holding status after drop
            // === PLAY DROP ANIMATION ===
            Debug.Log($"Dropp Animation Played here ---> (1)");
            AyoPotTrigger potScript = currentPot.potObject.GetComponent<AyoPotTrigger>();
            if (potScript != null)
            {
                potScript.TriggerStoneDropHighlight();
            }

            if (currentPlayer == player1 && SoundManager.Instance != null)
            {
                 SoundManager.Instance.PlayStoneDrop(potWasEmpty);
                yield return StartCoroutine(handMovement.PlayDropAnimation(stonesInHand.Count > 0));
            }
            if (currentPlayer == computer && SoundManager.Instance != null)
            {
                 SoundManager.Instance.PlayStoneDrop(potWasEmpty);
                yield return StartCoroutine(aiMovement.PlayDropAnimation(stonesInHand.Count > 0));
            }

            // Check rule 4: Last stone and pot is empty
            if (isLastStone && currentPot.IsEmpty)
            {
                // Enable stone visual and place in pot
                if (stone.visualObject != null)
                {
                    stone.visualObject.SetActive(true);
                }

                // === DROP ANIMATION (no more stones) ===
                // yield return StartCoroutine(handMovement.PlayDropAnimation(false));

                currentPot.AddStone(stone);

                if (uiManager != null)
                {
                    uiManager.UpdateStonesInHandDisplay(stonesInHand);
                }
                if (currentPlayer == player1 && SoundManager.Instance != null)
                {
                    // SoundManager.Instance.PlayStoneDrop(potWasEmpty);
                    handMovement.SetIdle();
                    ReturnPlayerHandToOriginal();
                }
                if (currentPlayer == computer && SoundManager.Instance != null)
                {
                    // SoundManager.Instance.PlayStoneDrop(potWasEmpty);
                    aiMovement.SetIdle();
                    ReturnComputerHandToOriginal();
                }
                Debug.Log($"Last stone placed in empty pot {currentPot.potID}. Turn ends.");


                EndTurn();
                yield break;
            }

            // Check rule 6: Capture rule (pot will have 4 stones after adding)
            if (currentPot.StoneCount == 3)
            {
                // Enable stone visual and place in pot
                if (stone.visualObject != null)
                {
                    stone.visualObject.SetActive(true);
                }
                if (uiManager != null)
                {
                    uiManager.UpdateStonesInHandDisplay(stonesInHand);
                }

                currentPot.AddStone(stone);
               

                Debug.Log($"Pot {currentPot.potID} now has 4 stones! CAPTURED!");
                yield return new WaitForSeconds(stoneDropDelay);
                if (currentPlayer == player1 && ScoreManager.Instance != null && uiManager != null && SoundManager.Instance != null)
                {
                    // SoundManager.Instance.PlayStoneDrop(potWasEmpty);
                    ScoreManager.Instance.AddPotCaptured();
                    uiManager.ShowPointsEarned();
                    uiManager.UpdateStonesInHandDisplay(stonesInHand);
                    SoundManager.Instance.PlayStoneCapture();
                    SoundManager.Instance.PlayPointScored();
                }
                if (currentPlayer == computer && SoundManager.Instance != null)
                {
                    // SoundManager.Instance.PlayStoneDrop(potWasEmpty);
                    SoundManager.Instance.PlayComputerPointScored();
                }

                // Capture all stones
                List<Stone> capturedStones = currentPot.TakeAllStones();

                // Move captured stones to player's capture area
                Transform captureContainer = currentPlayer == player1 ?
                    capturedStonesPlayer1Container : capturedStonesComputerContainer;

                foreach (var capturedStone in capturedStones)
                {
                    if (capturedStone.visualObject != null && captureContainer != null)
                    {
                        capturedStone.visualObject.SetActive(true);
                        capturedStone.visualObject.transform.SetParent(captureContainer);
                    }
                }

                currentPlayer.CaptureStones(capturedStones);
                lastPlayerToCapture = currentPlayer; // Track who captured
                ArrangeCapturedStones(currentPlayer);
                
                Debug.Log($"{currentPlayer.playerType} captured {capturedStones.Count} stones!");
                yield return new WaitForSeconds(stonePickupDelay);

                // If last stone, end turn
                if (isLastStone)
                {
                    if (uiManager != null)
                    {
                        uiManager.UpdateStonesInHandDisplay(stonesInHand);
                    }

                    // yield return StartCoroutine(ReturnHandToOriginalPosition(activeHand));
                    //     handMovement.SetIdle();
                    if (currentPlayer == player1)
                    {
                        handMovement.SetIdle();
                       ReturnPlayerHandToOriginal();
                    }
                    if (currentPlayer == computer)
                    {

                        aiMovement.SetIdle();
                        ReturnComputerHandToOriginal();
                    }
                    EndTurn();
                    yield break;
                }
                continue;
            }

            // Check rule 5: Last stone in non-empty pot (not 3 stones)
            if (isLastStone && !currentPot.IsEmpty)
            {
                // Add the stone first
                if (stone.visualObject != null)
                {
                    stone.visualObject.SetActive(true);
                }
                
                // === DROP ANIMATION (will pickup more stones after) ===
                // yield return StartCoroutine(handMovement.PlayDropAnimation(false));
                currentPot.AddStone(stone);
               
                // Debug.Log($"Pick Up Animation Played here ---> (2)");
                if (currentPlayer == player1 && SoundManager.Instance != null)
                {
                    // SoundManager.Instance.PlayStoneDrop(false);
                    yield return StartCoroutine(handMovement.PlayPickUpAnimation());
                    SoundManager.Instance.PlayStonePickup();
                }

                if (currentPlayer == computer && SoundManager.Instance != null)
                {
                    // SoundManager.Instance.PlayStoneDrop(false);
                    yield return StartCoroutine(aiMovement.PlayPickUpAnimation());
                     SoundManager.Instance.PlayStonePickup();
                }

                // Pick up all stones and continue
                stonesInHand = currentPot.TakeAllStones();
                // Hide stones again
                foreach (var s in stonesInHand)
                {
                    if (s.visualObject != null)
                    {
                        s.visualObject.SetActive(false);
                    }
                }

                if (uiManager != null)
                {
                    uiManager.UpdateStonesInHandDisplay(stonesInHand);
                }
                yield return new WaitForSeconds(stonePickupDelay);
                // Update UI with new stones
                // if (handStoneUI != null)
                // {
                //     handStoneUI.UpdateStoneDisplay(stonesInHand);
                // }
                continue;
            }

            // Normal placement
            if (stone.visualObject != null)
            {
                stone.visualObject.SetActive(true);
            }

            bool hasMoreStones = stonesInHand.Count > 0;

            // === DROP ANIMATION (pass whether more stones remain) ===
            // yield return StartCoroutine(handMovement.PlayDropAnimation(hasMoreStones));

            currentPot.AddStone(stone);

            if (uiManager != null)
            {
                uiManager.UpdateStonesInHandDisplay(stonesInHand);
            }

            Debug.Log($"Placed stone in {currentPot.potID} ({currentPot.StoneCount} stones)");

            // Visual delay
            yield return new WaitForSeconds(stoneDropDelay);
        }
        if (currentPlayer == player1)
        {
            handMovement.SetIdle();
            ReturnPlayerHandToOriginal();
        }
        if (currentPlayer == computer)
        {
            aiMovement.SetIdle();
            ReturnComputerHandToOriginal();
        }

        // Return hand to original position
        // yield return StartCoroutine(ReturnHandToOriginalPosition(activeHand));


        // Should not reach here, but safety
        EndTurn();
    }

    private IEnumerator MoveHandToPot(Pot pot, Transform handTransform)
    {
        if (handTransform == null || pot.stoneContainer == null) yield break;
        Debug.Log($"Hand Type: {handTransform.name} hand to {handTransform}");
        // {
        //     handTransform.localPosition = new Vector2(0, -0.5f);
        // }
        // Convert to 2D position (using x and y, ignore z for 2D movement)
        Vector2 targetPosition2D = new Vector2(
            pot.stoneContainer.position.x,
            handTransform.name == "HumanPlayer" ? pot.stoneContainer.position.y - 0.5f : pot.stoneContainer.position.y + 0.5f
        );

        while (Vector2.Distance((Vector2)handTransform.position, targetPosition2D) > 0.1f)
        {
            Vector2 currentPos2D = handTransform.position;
            Vector2 newPos2D = Vector2.MoveTowards(
                currentPos2D,
                targetPosition2D,
                handMoveSpeed * Time.deltaTime
            );

            // Apply 2D position (keep original z)
            handTransform.position = new Vector3(newPos2D.x, newPos2D.y, handTransform.position.z);
            yield return null;
        }

        handTransform.position = new Vector3(targetPosition2D.x, targetPosition2D.y, handTransform.position.z);
    }


    public void ReturnPlayerHandToOriginal()
        {
            StartCoroutine(ReturnHandToOriginalPosition(playerHandTransform, true));
        }
        
        public void ReturnComputerHandToOriginal()
        {
            StartCoroutine(ReturnHandToOriginalPosition(computerHandTransform, false));
        }
    
    private IEnumerator ReturnHandToOriginalPosition(Transform handTransform, bool isPlayerHand)
    {
        if (handTransform == null) yield break;
        
        bool isUIElement = isPlayerHand ? playerHandIsUI : computerHandIsUI;
        
        if (isUIElement)
        {
            // Handle UI element (RectTransform)
            RectTransform rectTransform = isPlayerHand ? playerHandRect : computerHandRect;
            Vector2 targetPosition = isPlayerHand ? originalHandAnchoredPosition : computerOriginalAnchoredPosition;
            
            yield return StartCoroutine(ReturnUIHandToPosition(rectTransform, targetPosition));
        }
        else
        {
            // Handle world object (Transform)
            Vector3 targetPosition = isPlayerHand ? originalHandLocalPosition : computerOriginalHandLocalPosition;
            
            yield return StartCoroutine(ReturnWorldHandToPosition(handTransform, targetPosition));
        }
    }
    
    // For UI elements (RectTransform)
    private IEnumerator ReturnUIHandToPosition(RectTransform rectTransform, Vector2 targetPosition)
    {
        if (rectTransform == null) yield break;
        
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.1f)
        {
            Vector2 newPos = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                targetPosition,
                handMoveSpeed * 100f * Time.deltaTime // Multiply by 100 for UI units
            );
            
            rectTransform.anchoredPosition = newPos;
            yield return null;
        }
        
        // Snap to exact position
        rectTransform.anchoredPosition = targetPosition;
        Debug.Log($"UI Hand returned to: {targetPosition}");
    }
    
    // For world objects (Transform)
    private IEnumerator ReturnWorldHandToPosition(Transform handTransform, Vector3 targetPosition)
    {
        if (handTransform == null) yield break;
        
        // Convert to world position for comparison
        Vector3 targetWorldPosition = handTransform.parent != null 
            ? handTransform.parent.TransformPoint(targetPosition)
            : targetPosition;
        
        while (Vector3.Distance(handTransform.position, targetWorldPosition) > 0.1f)
        {
            handTransform.position = Vector3.MoveTowards(
                handTransform.position,
                targetWorldPosition,
                handMoveSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        // Snap to exact local position
        handTransform.localPosition = targetPosition;
        Debug.Log($"World Hand returned to local: {targetPosition}");
    }
    
    // Public method to manually reset positions (useful for debugging)
    public void ResetHandPositions()
    {
        if (playerHandTransform != null)
        {
            if (playerHandIsUI && playerHandRect != null)
            {
                playerHandRect.anchoredPosition = originalHandAnchoredPosition;
            }
            else
            {
                playerHandTransform.localPosition = originalHandLocalPosition;
            }
        }
        
        if (computerHandTransform != null)
        {
            if (computerHandIsUI && computerHandRect != null)
            {
                computerHandRect.anchoredPosition = computerOriginalAnchoredPosition;
            }
            else
            {
                computerHandTransform.localPosition = computerOriginalHandLocalPosition;
            }
        }
        
        Debug.Log("Hand positions reset to original");
    }

    private void ArrangeCapturedStones(Player player)
    {
        Transform container = player == player1 ? capturedStonesPlayer1Container : capturedStonesComputerContainer;
        if (container == null) return;

        List<Stone> stones = player.capturedStones;
        int columns = 6;
        float spacing = 0.2f;

        for (int i = 0; i < stones.Count; i++)
        {
            if (stones[i].visualObject != null)
            {
                int row = i / columns;
                int col = i % columns;

                // Use 2D positioning (x and y only)
                Vector2 position2D = new Vector2(
                    container.position.x + col * spacing,
                    container.position.y + row * spacing
                );

                stones[i].visualObject.transform.position = new Vector3(
                    position2D.x,
                    position2D.y,
                    stones[i].visualObject.transform.position.z
                );
            }
        }
    }

    // ========== TURN MANAGEMENT ==========
    private void EndTurn()
    {
        Debug.Log($"{currentPlayer.playerType} turn ended. Captured stones: {currentPlayer.CapturedStoneCount}");

        // Hide UI
        if (uiManager != null)
        {
            uiManager.HideStonesInHand();
        }

        // Check if only 4 or fewer stones remain on board (Rule 1)
        int remainingStonesOnBoard = GetTotalStonesOnBoard();
        if (remainingStonesOnBoard <= 4 && lastPlayerToCapture != null)
        {
            Debug.Log($"Only {remainingStonesOnBoard} stones left on board. {lastPlayerToCapture.playerType} takes remaining stones!");
            CaptureAllRemainingStones(lastPlayerToCapture);

            // Award points for capturing remaining stones (only for Player 1)
            if (lastPlayerToCapture == player1 && ScoreManager.Instance != null && uiManager != null && SoundManager.Instance != null)
            {
                ScoreManager.Instance.AddRemainingCaptured();
                uiManager.ShowPointsEarned();
                 SoundManager.Instance.PlayStoneCapture();
                SoundManager.Instance.PlayPointScored();
            }
   
            if (lastPlayerToCapture == computer && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayComputerPointScored();
            }

            EndRound();
            return;
        }

        // Check if round is over (all pots empty)
        if (AreAllPotsEmpty())
        {
            EndRound();
            return;
        }

        // Switch players
        currentPlayer = (currentPlayer == player1) ? computer : player1;

        if (!HasValidMoves(currentPlayer))
        {
            Debug.Log($"{currentPlayer.playerType} has no valid moves! Skipping turn...");
            EndTurn(); // Recursively switch to next player
            return;
        }

        if (currentPlayer.playerType == PlayerType.Computer)
        {
            currentState = GameState.ComputerThinking;
            StartCoroutine(ComputerTurn());
        }
        else
        {
            currentState = GameState.WaitingForPlayerSelection;
            Debug.Log("Player 1's turn!");
        }
    }

private bool HasValidMoves(Player current_player)
    {
        foreach (string potID in current_player.controlledPots)
        {
            Pot pot = GetPotByID(potID);
            if (pot != null && !pot.IsEmpty)
            {
                return true; // Found at least one pot with stones
            }
        }
        return false; // No pots with stones
    }
    private int GetTotalStonesOnBoard()
    {
        int total = 0;
        foreach (var pot in allPots)
        {
            total += pot.StoneCount;
        }
        return total;
    }

    private void CaptureAllRemainingStones(Player capturingPlayer)
    {
        Transform captureContainer = capturingPlayer == player1 ?
            capturedStonesPlayer1Container : capturedStonesComputerContainer;

        foreach (var pot in allPots)
        {
            if (!pot.IsEmpty)
            {
                List<Stone> remainingStones = pot.TakeAllStones();

                foreach (var stone in remainingStones)
                {
                    if (stone.visualObject != null && captureContainer != null)
                    {
                        stone.visualObject.SetActive(true);
                        stone.visualObject.transform.SetParent(captureContainer);
                    }
                }

                capturingPlayer.CaptureStones(remainingStones);
            }
        }

        ArrangeCapturedStones(capturingPlayer);
    }

    private IEnumerator ComputerTurn()
    {
        Debug.Log("Computer is thinking...");
        yield return new WaitForSeconds(computerThinkTime);

        // Get valid pots for computer
        List<Pot> validPots = new List<Pot>();
        foreach (string potID in computer.controlledPots)
        {
            Pot pot = GetPotByID(potID);
            if (pot != null && !pot.IsEmpty)
            {
                validPots.Add(pot);
            }
        }

        if (validPots.Count == 0)
        {
            Debug.Log("Computer has no valid moves!");

            if (currentPlayer == computer)
            {
                aiMovement.SetIdle();
            }
            EndTurn();
            yield break;
        }

        // Select pot based on difficulty
        selectedPot = SelectPotBasedOnDifficulty(validPots);
        Debug.Log($"Computer selected {selectedPot.potID}");
        AyoPotTrigger potScript = selectedPot.potObject.GetComponent<AyoPotTrigger>();
        if (potScript != null && SoundManager.Instance != null)
        {
            potScript.TriggerComputerSelection();
              SoundManager.Instance.PlayPotSelect();
        }
        
        // Wait a moment to show the selection
    

        StartMove();
    }

    private Pot SelectPotBasedOnDifficulty(List<Pot> validPots)
    {
        switch (aiDifficulty)
        {
            case Difficulty.Easy:
                // Random selection
                return validPots[Random.Range(0, validPots.Count)];

            case Difficulty.Medium:
                // Prefer pots that lead to captures
                Pot capturePot = FindPotLeadingToCapture(validPots);
                if (capturePot != null) return capturePot;
                // Otherwise random
                return validPots[Random.Range(0, validPots.Count)];

            case Difficulty.Hard:
                // Best move: maximize captures, minimize opponent opportunities
                return FindBestMove(validPots);

            default:
                return validPots[Random.Range(0, validPots.Count)];
        }
    }

    private Pot FindPotLeadingToCapture(List<Pot> validPots)
    {
        foreach (var pot in validPots)
        {
            int potIndex = allPots.IndexOf(pot);
            int stones = pot.StoneCount;

            // Simulate distribution (clockwise)
            for (int i = 1; i <= stones; i++)
            {
                int targetIndex = (potIndex - i + allPots.Count) % allPots.Count;
                Pot targetPot = allPots[targetIndex];

                // Check if this would create a capture (3 stones + 1 = 4)
                if (i == stones && targetPot.StoneCount == 3)
                {
                    return pot;
                }
            }
        }
        return null;
    }

    private Pot FindBestMove(List<Pot> validPots)
    {
        Pot bestPot = null;
        int bestScore = int.MinValue;

        foreach (var pot in validPots)
        {
            int score = EvaluateMove(pot);
            if (score > bestScore)
            {
                bestScore = score;
                bestPot = pot;
            }
        }

        return bestPot ?? validPots[0];
    }

    private int EvaluateMove(Pot pot)
    {
        int score = 0;
        int potIndex = allPots.IndexOf(pot);
        int stones = pot.StoneCount;

        // Simulate distribution (clockwise)
        for (int i = 1; i <= stones; i++)
        {
            int targetIndex = (potIndex - i + allPots.Count) % allPots.Count;
            Pot targetPot = allPots[targetIndex];

            // High score for captures
            if (i == stones && targetPot.StoneCount == 3)
            {
                score += 10;
            }

            // Bonus for landing on opponent's side
            if (targetPot.potID.EndsWith("A"))
            {
                score += 1;
            }

            // Avoid giving opponent easy captures
            if (targetPot.StoneCount == 2 && targetPot.potID.EndsWith("A"))
            {
                score -= 5;
            }
        }

        return score;
    }

    // ========== ROUND END ==========
    private void EndRound()
    {
        currentState = GameState.RoundEnd;
        Debug.Log("=== ROUND ENDED ===");
        int player1Score = player1.CapturedStoneCount;
        int computerScore = computer.CapturedStoneCount;

        Debug.Log($"Player 1 captured: {player1Score} stones");
        Debug.Log($"Computer captured: {computerScore} stones");

        // Determine round winner (Rule 2)
        string roundResult = "";
        if (player1Score > computerScore)
        {
            player1.roundsWon++;
            roundResult = "win";
            Debug.Log("Player 1 wins this round!");
        }
        else if (computerScore > player1Score)
        {
            computer.roundsWon++;
            roundResult = "lose";
            Debug.Log("Computer wins this round!");
        }
        else
        {
            roundResult = "draw";
            Debug.Log("Round is a draw!");
        }

        Debug.Log($"Score: Player 1 {player1.roundsWon} - Computer {computer.roundsWon}");

        // Award points based on round result (only for Player 1)
        if (ScoreManager.Instance != null)
        {
            switch (roundResult.ToLower())
            {
                case "win":
                    ScoreManager.Instance.AddRoundWon();
                    break;
                case "lose":
                    ScoreManager.Instance.AddRoundLost();
                    break;
                case "draw":
                    ScoreManager.Instance.AddRoundDraw();
                    break;
            }
        }


        // Show round result notification
        if (SoundManager.Instance != null)
        {
            switch (roundResult.ToLower())
            {
                case "win":
                    SoundManager.Instance.PlayRoundWin();
                    break;
                case "lose":
                    SoundManager.Instance.PlayRoundLose();
                    break;
                case "draw":
                    SoundManager.Instance.PlayRoundDraw();

                    break;
            }
        }

        if (uiManager != null)
        {
            uiManager.ShowPointsEarned();
            uiManager.ShowRoundResult(roundResult, player1Score, computerScore,
                                      player1.roundsWon, computer.roundsWon);
        }

        // Clear captured stones visually AND from data
        ClearCapturedStones(player1);
        ClearCapturedStones(computer);
        lastPlayerToCapture = null;

        // Update round score in UI
        if (uiManager != null)
        {
            uiManager.UpdateRoundScore(player1.roundsWon, computer.roundsWon);
        }

        // Check if game is over (Rule 3: best of 5)
        if (currentRound >= maxRounds)
        {
            // Delay game over to show round result first
            StartCoroutine(DelayedGameOver());
            return;
        }

        // Start new round after delay
        StartCoroutine(DelayedRoundStart());
    }

    private IEnumerator DelayedRoundStart()
    {
        // Wait for round result to be shown (should match UI auto-hide delay)
        yield return new WaitForSeconds(roundResultDisplayTime);

        currentRound++;
        Debug.Log($"Starting Round {currentRound}...");

        // Show round start notification
        if (uiManager != null)
        {
            uiManager.SetRoundNumber(currentRound, maxRounds);
            uiManager.ShowRoundStart(currentRound);
        }

        // Play round start sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayRoundStart();
        }

        // Wait for round start notification
        yield return new WaitForSeconds(roundStartDisplayTime);
        InitializeStones();
        currentPlayer = player1;
        currentState = GameState.WaitingForPlayerSelection;
        Debug.Log("Round ready! Player 1's turn!");

        if (currentRound % 2 == 0)
        {
            currentPlayer = computer;
            currentState = GameState.ComputerThinking;
            Debug.Log("Computer starts this round!");
            StartCoroutine(ComputerTurn());
        }
        else
        {
            currentPlayer = player1;
            currentState = GameState.WaitingForPlayerSelection;
            Debug.Log("Player starts this round!");
        }
    }

    private IEnumerator DelayedGameOver()
    {
        // Wait for round result to be shown
        yield return new WaitForSeconds(roundResultDisplayTime);

        DetermineGameWinner();
    }

    private void ClearCapturedStones(Player player)
    {
        // Destroy visual stone objects
        foreach (var stone in player.capturedStones)
        {
            if (stone.visualObject != null)
            {
                Destroy(stone.visualObject);
            }
        }

        // Clear the list
        player.capturedStones.Clear();
    }

    private void DetermineGameWinner()
    {
        Debug.Log("=== GAME OVER ===");

        Player gameWinner = null;

        if (player1.roundsWon > computer.roundsWon)
        {
            gameWinner = player1;
        }
        else if (computer.roundsWon > player1.roundsWon)
        {
            gameWinner = computer;
        }
        else
        {
            gameWinner = null;
        }
        // If tied, could use total stones as tiebreaker or declare draw

        GameOver(gameWinner);
    }

    private void CheckTerritoryConquest()
    {
        // This rule is now replaced by round-based winning
        // Keep for potential future use
    }

    private void GameOver(Player winner)
    {
        currentState = GameState.GameOver;

        // Award points for game result (only for Player 1)
        if (ScoreManager.Instance != null)
        {
            if (winner == player1)
            {
                ScoreManager.Instance.AddGameWon();
            }
            else if (winner == computer)
            {
                ScoreManager.Instance.AddGameLost();
            }
            else // Draw
            {
                ScoreManager.Instance.AddGameDraw();
            }
        }

        // Play game over sound
        if (SoundManager.Instance != null)
        {
            if (winner != null)
            {
                if (winner == player1)
                {
                    SoundManager.Instance.PlayGameWin();

                }
                else
                {
                    SoundManager.Instance.PlayGameLose();
                }
            }
            else
            {
                SoundManager.Instance.PlayGameDraw();

            }
        }

        if (uiManager != null)
        {
            uiManager.ShowPointsEarned();
        }

        if (winner != null)
        {
            Debug.Log($"=== GAME OVER === {winner.playerType} WINS!");
            Debug.Log($"Final Score: Player 1 {player1.roundsWon} - Computer {computer.roundsWon}");
        }
        else
        {
            Debug.Log("=== GAME OVER === IT'S A DRAW!");
        }

        // TODO: Show game over UI
    }

    // ========== UTILITY ==========
    private Pot GetPotByID(string potID)
    {
        return allPots.Find(p => p.potID == potID);
    }

    private bool AreAllPotsEmpty()
    {
        foreach (var pot in allPots)
        {
            if (!pot.IsEmpty) return false;
        }
        return true;
    }

    // ========== PUBLIC METHODS FOR UI ==========
    public string GetCurrentPlayerName()
    {
        if (currentPlayer == null) return "Initializing...";
        return currentPlayer.playerType.ToString();
    }

    public int GetPlayer1Score()
    {
        if (player1 == null) return 0;
        return player1.CapturedStoneCount;
    }

public int GetPlayerScore()
    {
        if (player1 == null) return 0;
        return player1.CapturedStoneCount;
    }
    public int GetComputerScore()
    {
        if (computer == null) return 0;
        return computer.CapturedStoneCount;
    }

    public int GetPlayer1Territory()
    {
        if (player1 == null) return 0;
        return player1.controlledPots.Count;
    }

    public int GetComputerTerritory()
    {
        if (computer == null) return 0;
        return computer.controlledPots.Count;
    }

    public int GetPlayer1RoundsWon()
    {
        if (player1 == null) return 0;
        return player1.roundsWon;
    }

    public int GetComputerRoundsWon()
    {
        if (computer == null) return 0;
        return computer.roundsWon;
    }

    public int[] GetCurrentRound()
    {
        int[] roundNums = { maxRounds, currentRound };
        return roundNums;
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }
}
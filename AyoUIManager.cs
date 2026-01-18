using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
public class AyoUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AyoGameManager gameManager;

    [Header("Main UI Panel")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backMenuButton;

    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI currentPlayerText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Image currentPlayerIndicator; // Visual indicator
    [SerializeField] private Sprite PlayerIcon;
    [SerializeField] private Sprite computerIcon;
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI computerScoreText;
    [SerializeField] private TextMeshProUGUI player1TerritoryText;
    [SerializeField] private TextMeshProUGUI computerTerritoryText;
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private TextMeshProUGUI pointsEarnedText;
    [SerializeField] private GameObject pointsNotification;
    [SerializeField] private GameObject starCoin;
    [SerializeField] private GameObject goldenCoin;

    [Header("Stones in Hand Display")]
    [SerializeField] private GameObject stonesInHandPanel;
    [SerializeField] private TextMeshProUGUI stonesInHandCountText;
    [SerializeField] private Transform stonesInHandContainer;
    [SerializeField] private GameObject stoneThumbnailPrefab;
    [SerializeField] private Sprite blueStoneSprite;
    [SerializeField] private Sprite blackStoneSprite;

    [Header("Game State Display")]
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private Image gameStateBanner; // Optional banner
    [SerializeField] private Sprite luckyIcon;
    [SerializeField] private Sprite knowledgeIcon;
    [SerializeField] private Sprite peaceIcon;
    [SerializeField] private Sprite wisdomIcon;
    [SerializeField] private Sprite personIcon;
    [SerializeField] private Sprite maskIcon;

    [Header("Round Info")]
    [SerializeField] private TextMeshProUGUI roundNumberText;
    [SerializeField] private TextMeshProUGUI player1RoundsWonText;
    [SerializeField] private TextMeshProUGUI computerRoundsWonText;

    [Header("Round Start Notification")]
    [SerializeField] private GameObject roundStartPanel;
    [SerializeField] private TextMeshProUGUI roundStartText;
    [SerializeField] private float roundStartDisplayTime = 4f;

    [Header("Round Result Notification")]
    [SerializeField] private GameObject roundResultPanel;
    [SerializeField] private Image roundResultBackgroundImage; // Background image
    [SerializeField] private Sprite winRoundImage;
    [SerializeField] private Sprite drawRoundImage;
    [SerializeField] private Sprite loseRoundImage;
    [SerializeField] private TextMeshProUGUI roundResultText;
    [SerializeField] private TextMeshProUGUI roundResultDetailText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float autoHideDelay = 3.5f; // Increased to hide before next round

    [Header("Game Over UI")]
    [SerializeField] private Sprite winImage;
    [SerializeField] private Sprite loseImage;
    [SerializeField] private Sprite drawImage;
    [SerializeField] private Image gameResultImage;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI gameScoreText;
    [SerializeField] private TextMeshProUGUI capturedStoneText;
    [SerializeField] private TextMeshProUGUI capturedPotText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button playButton;

    [Header("Visual Settings")]
    [SerializeField] private Color player1Color = new Color(0.4f, 0.9f, 1f); // Blue
    [SerializeField] private Color computerColor = new Color(1f, 0.3f, 0.3f); // Red
    [SerializeField] private Vector2 thumbnailSize = new Vector2(50, 50);

    [Header("Settings Menu")]
    [SerializeField] private SettingsMenu settingsMenu;

    private List<GameObject> activeThumbnails = new List<GameObject>();
    private void Start()
    {
        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Hide stones in hand panel initially
        HideStonesInHand();
        // Hide round result panel initially
        if (roundResultPanel != null)
        {
            roundResultPanel.SetActive(false);
        }

        // Hide round start panel initially
        if (roundStartPanel != null)
        {
            roundStartPanel.SetActive(false);
        }

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }
        if (backMenuButton != null)
        {
            backMenuButton.onClick.AddListener(OnBackMenuButtonClicked);
        }
        if(playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        if (pointsNotification != null)
        {
            pointsNotification.SetActive(false);
        }
        if (goldenCoin != null)
        {
            goldenCoin.SetActive(false);
        }

        // Don't call UpdateUI here - wait for game to initialize
    }

   

    private void Update()
    {
        if (gameManager == null) return;
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateCurrentPlayer();
        UpdateScores();
        UpdateGameState();
        UpdateRoundNumber();
        UpdateTotalPoints();

        // Check for game over
        if (gameManager.GetCurrentState() == GameState.GameOver)
        {
            ShowGameOver();
        }
    }

    // ========== CURRENT PLAYER ==========
    private void UpdateCurrentPlayer()
    {
        if (gameManager == null) return;

        string playerName = gameManager.GetCurrentPlayerName();
        GameState state = gameManager.GetCurrentState();

        // Update player text
        if (currentPlayerText != null)
        {
            currentPlayerText.text = $"Current Turn: {playerName}";
            currentPlayerText.color = playerName == "Player1" ? player1Color : computerColor;
        }

        // Update indicator color
        if (currentPlayerIndicator != null)
        {
            currentPlayerIndicator.color = playerName == "Player1" ? player1Color : computerColor;
            currentPlayerIndicator.sprite = playerName == "Player1" ? PlayerIcon : computerIcon;
        }

        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = GetInstructionText(state, playerName);
        }
    }

    private string GetInstructionText(GameState state, string playerName)
    {
        switch (state)
        {
            case GameState.WaitingForPlayerSelection:
                return "Click on a pot to select";
            case GameState.PlayerMoving:
                return "Distributing stones...";
            case GameState.ComputerThinking:
                return "Computer is thinking...";
            case GameState.ComputerMoving:
                return "Computer is playing...";
            case GameState.RoundEnd:
                return "Round ended! Starting new round...";
            case GameState.GameOver:
                return "Game Over!";
            default:
                return "";
        }
    }

    // ========== SCORES ==========
    private void UpdateScores()
    {
        if (gameManager == null) return;

        int player1Score = gameManager.GetPlayer1Score();
        int computerScore = gameManager.GetComputerScore();
        int player1Territory = gameManager.GetPlayer1Territory();
        int computerTerritory = gameManager.GetComputerTerritory();

        // Update score texts
        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"{player1Score}";
        }

        if (computerScoreText != null)
        {
            computerScoreText.text = $"{computerScore}";
        }

        // Update territory texts
        if (player1TerritoryText != null)
        {
            player1TerritoryText.text = $"{player1Territory} pots";
        }

        if (computerTerritoryText != null)
        {
            computerTerritoryText.text = $"{computerTerritory} pots";
        }
    }

    // ========== TOTAL POINTS ==========
    private void UpdateTotalPoints()
    {
        if (ScoreManager.Instance == null) return;

        if (totalPointsText != null)
        {
            totalPointsText.text = $"{ScoreManager.Instance.TotalPoints}";
        }
    }

    public void ShowPointsEarned()
    {
        if (pointsEarnedText != null)
        {
            pointsEarnedText.text = $"+{ScoreManager.Instance.PointsEarned}";
        }

        if (pointsNotification == null)
            return;

        Debug.Log($"points earned received -- {pointsEarnedText.text}");

        // Cancel previous tweens
        LeanTween.cancel(pointsNotification);

        pointsNotification.SetActive(true);

        // Get RectTransform
        RectTransform notifRect = pointsNotification.GetComponent<RectTransform>();
        notifRect.anchoredPosition = Vector2.zero;
        notifRect.localScale = Vector3.zero;

        // Get or add CanvasGroup
        CanvasGroup canvasGroup = pointsNotification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = pointsNotification.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // Fade + scale in
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.4f);
        LeanTween.scale(notifRect, Vector3.one, 0.4f)
            .setEaseOutBack();

        // ✅ FIXED: Use LeanTween.value to animate anchoredPosition
        LeanTween.value(pointsNotification, Vector2.zero, new Vector2(-126f, -800f), 1f)
            .setOnUpdate((Vector2 val) =>
            {
                if (notifRect != null)
                    notifRect.anchoredPosition = val;
            })
            .setEase(LeanTweenType.easeInCubic)
            .setDelay(0.3f);

        // --------------------
        // GOLD COIN (2D UI)
        // --------------------
        if (goldenCoin != null)
        {
            LeanTween.cancel(goldenCoin);
            goldenCoin.SetActive(true);

            // Get RectTransform
            RectTransform coinRect = goldenCoin.GetComponent<RectTransform>();
            coinRect.anchoredPosition = new Vector2(200f, 150f);
            coinRect.localScale = Vector3.zero; // Start scaled down

            // Get or add CanvasGroup
            CanvasGroup coinCanvas = goldenCoin.GetComponent<CanvasGroup>();
            if (coinCanvas == null)
                coinCanvas = goldenCoin.AddComponent<CanvasGroup>();

            coinCanvas.alpha = 0f;

            // Fade in
            LeanTween.alphaCanvas(coinCanvas, 1f, 0.4f);

            // Scale up
            LeanTween.scale(coinRect, new Vector2(20f, 20f), 0.4f)
                .setEaseOutBack();

            // // Rotate around Z axis (2D)
            // LeanTween.rotateZ(goldenCoin, 360f, 1.5f)
            //     .setLoopClamp();

            // ✅ FIXED: Animate anchoredPosition for 2D UI
            Vector2 startPos = new Vector2(0f, 33f);
            Vector2 endPos = new Vector2(-126f, -800f);

            LeanTween.value(goldenCoin, startPos, endPos, 1f)
                .setOnUpdate((Vector2 val) =>
                {
                    if (coinRect != null)
                        coinRect.anchoredPosition = val;
                })
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(0.4f);
        }

        StartCoroutine(HidePointsNotification());
        UpdateTotalPoints();
    }



    private IEnumerator HidePointsNotification()
    {
        yield return new WaitForSeconds(2f);

        // Hide UI notification
        if (pointsNotification != null)
        {
            CanvasGroup notificationCanvasGroup = pointsNotification.GetComponent<CanvasGroup>();
            if (notificationCanvasGroup != null)
            {
                LeanTween.alphaCanvas(notificationCanvasGroup, 0f, 0.8f);
            }

            LeanTween.moveLocalY(pointsNotification, 200f, 0.8f)
                .setEase(LeanTweenType.easeInCubic)
                .setOnComplete(() =>
                {
                    if (pointsNotification != null)
                    {
                        pointsNotification.SetActive(false);
                    }
                });

            if (goldenCoin != null)
            {
                goldenCoin.SetActive(false);

                // Stop rotation
                if (starCoin != null)
                {
                    LeanTween.cancel(starCoin);
                }
            }
        }



    }

    // ========== GAME STATE ==========
    private void UpdateGameState()
    {
        if (gameManager == null || gameStateText == null) return;

        GameState state = gameManager.GetCurrentState();
        gameStateText.text = GetStateMessage(state);

        // Update banner color if exists
        if (gameStateBanner != null)
        {
            gameStateBanner.color = GetStateBannerColor(state);
            gameStateBanner.sprite = GetStateBannerIcon(state);
        }
    }

    private string GetStateMessage(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingForPlayerSelection:
                return "Your Turn";
            case GameState.PlayerMoving:
                return "Playing...";
            case GameState.ComputerThinking:
                return "Computer Thinking...";
            case GameState.ComputerMoving:
                return "Computer Playing...";
            case GameState.RoundEnd:
                return "Round Complete";
            case GameState.GameOver:
                return "Game Over";
            default:
                return "";
        }
    }

    private Color GetStateBannerColor(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingForPlayerSelection:
                return player1Color;
            case GameState.ComputerThinking:
            case GameState.ComputerMoving:
                return computerColor;
            case GameState.RoundEnd:
                return Color.yellow;
            case GameState.GameOver:
                return Color.gray;
            default:
                return Color.white;
        }
    }

    private Sprite GetStateBannerIcon(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingForPlayerSelection:
                return personIcon;
            case GameState.ComputerThinking:
                return wisdomIcon;
            case GameState.ComputerMoving:
                return luckyIcon;
            case GameState.RoundEnd:
                return peaceIcon;
            case GameState.GameOver:
                return knowledgeIcon;
            default:
                return wisdomIcon;
        }
    }

    // ========== STONES IN HAND ==========
    public void ShowStonesInHand(List<Stone> stones)
    {
        if (stonesInHandPanel != null)
        {
            stonesInHandPanel.SetActive(true);
        }

        UpdateStonesInHandDisplay(stones);
    }

    public void UpdateStonesInHandDisplay(List<Stone> stones)
    {
        // Clear existing thumbnails
        ClearThumbnails();

        if (stones == null)
        {
            HideStonesInHand();
            return;
        }

        // Update count text
        if (stonesInHandCountText != null)
        {
            stonesInHandCountText.text = $"Stones in Hand: {stones.Count}";
        }

        // Create thumbnails
        if (stonesInHandContainer != null && stoneThumbnailPrefab != null)
        {
            for (int i = 0; i < stones.Count; i++)
            {
                CreateStoneThumbnail(stones[i].color, i);
            }
        }
    }

    private void CreateStoneThumbnail(StoneColor color, int index)
    {
        GameObject thumbnail = Instantiate(stoneThumbnailPrefab, stonesInHandContainer);

        // Set sprite
        Image img = thumbnail.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = color == StoneColor.Blue ? blueStoneSprite : blackStoneSprite;
        }

        // Set fixed size
        RectTransform rect = thumbnail.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(40, 40);
        }

        // Add layout element
        var layoutElement = thumbnail.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = thumbnail.AddComponent<LayoutElement>();
        }
        layoutElement.preferredWidth = thumbnailSize.x;
        layoutElement.preferredHeight = thumbnailSize.y;
        layoutElement.flexibleWidth = 0;
        layoutElement.flexibleHeight = 0;

        activeThumbnails.Add(thumbnail);
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

    public void HideStonesInHand()
    {
        //if (stonesInHandPanel != null)
        //{
        //   stonesInHandPanel.SetActive(false);
        //}
        ClearThumbnails();
    }

    // ========== ROUND START NOTIFICATION ==========
    public void ShowRoundStart(int roundNumber)
    {
        if (roundStartPanel == null) return;

        roundStartPanel.SetActive(true);

        if (roundStartText != null)
        {
            roundStartText.text = $"ROUND {roundNumber}";
            roundStartText.transform.localScale = Vector2.zero;
            LeanTween.scale(roundStartText.gameObject, Vector2.one, 0.5f)
                .setEaseOutBack();
        }

        // Auto-hide after display time
        StartCoroutine(AutoHideRoundStart());
    }

    private IEnumerator AutoHideRoundStart()
    {
        yield return new WaitForSeconds(roundStartDisplayTime);
        HideRoundStart();
    }

    public void HideRoundStart()
    {
        if (roundStartPanel != null)
        {
            roundStartPanel.SetActive(false);
        }
    }

    // ========== ROUND RESULT NOTIFICATION ==========
    public void ShowRoundResult(string result, int player1Score, int computerScore, int player1Rounds, int computerRounds)
    {
        if (roundResultPanel == null) return;
        SoundManager.Instance.PauseBackgroundMusic();
        roundResultPanel.SetActive(true);
        roundResultPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(roundResultPanel, Vector3.one, 0.5f)
                    .setEaseOutBack();
        // Determine result text and color
        string resultText = "";
        Color resultColor = Color.white;
        Sprite backgroundSprite = null;

        switch (result.ToLower())
        {
            case "win":
                resultText = "YOU WON THIS ROUND!";
                resultColor = player1Color;
                backgroundSprite = winRoundImage;
                break;
            case "lose":
                resultText = "YOU LOST THIS ROUND!";
                resultColor = computerColor;
                backgroundSprite = loseRoundImage;
                break;
            case "draw":
                resultText = "ROUND DRAW!";
                resultColor = Color.yellow;
                backgroundSprite = drawRoundImage;
                break;
        }

        // Set background image
        if (roundResultBackgroundImage != null && backgroundSprite != null)
        {
            roundResultBackgroundImage.sprite = backgroundSprite;
            roundResultBackgroundImage.enabled = true;
            LeanTween.scale(roundResultBackgroundImage.rectTransform,
                                new Vector3(0.8f, 0.8f, 1f),
                                1.5f)
                                .setDelay(0.2f)
                                .setEase(LeanTweenType.easeOutElastic);
        }

        if (roundResultText != null)
        {
            int[] roundNums = gameManager.GetCurrentRound();
            roundResultText.text = $"Round {roundNums[1]} Ends";
            roundResultText.color = resultColor;
            LeanTween.alpha(roundResultText.rectTransform, 1f, 0.5f).setDelay(0.5f);
        }

        if (roundResultDetailText != null)
        {
            roundResultDetailText.text = $"Player: {player1Score} stones\n" +
                                         $"Computer: {computerScore} stones\n\n" +
                                         $"Rounds Won\n" +
                                         $"Player: {player1Rounds} | Computer: {computerRounds}";
        }

        // Auto-hide after delay or wait for continue button
        if (continueButton != null && continueButton.gameObject.activeSelf)
        {
            // Manual mode - wait for button click
            continueButton.gameObject.SetActive(true);
            LeanTween.scale(continueButton.GetComponent<RectTransform>(),
                    new Vector3(1f, 1f, 1f),
                    1.6f)
                    .setDelay(0.2f)
                    .setEase(LeanTweenType.easeInElastic);
        }

        // Auto mode - hide after delay
        StartCoroutine(AutoHideRoundResult());
    }

    private IEnumerator AutoHideRoundResult()
    {
        yield return new WaitForSeconds(autoHideDelay);
        HideRoundResult();
    }

    public void HideRoundResult()
    {
        if (roundResultPanel != null)
        {
            SoundManager.Instance.ResumeBackgroundMusic();
            roundResultPanel.SetActive(false);
        }
    }

    private void OnContinueClicked()
    {
        HideRoundResult();
    }

    // ========== ROUND NUMBER ==========
    private void UpdateRoundNumber()
    {
        if (gameManager == null) return;

        if (roundNumberText != null)
        {
            int[] roundNums = gameManager.GetCurrentRound();
            roundNumberText.text = $"Round {roundNums[1]}/{roundNums[0]}";
        }

        // Update rounds won
        if (player1RoundsWonText != null)
        {
            int roundsWon = gameManager.GetPlayer1RoundsWon();
            player1RoundsWonText.text = $"Rounds Won: {roundsWon}";
        }

        if (computerRoundsWonText != null)
        {
            int roundsWon = gameManager.GetComputerRoundsWon();
            computerRoundsWonText.text = $"Rounds Won: {roundsWon}";
        }
    }

    public void SetRoundNumber(int round, int maxRounds)
    {
        if (roundNumberText != null)
        {
            roundNumberText.text = $"Round {round}/{maxRounds}";
        }
    }

    public void UpdateRoundScore(int player1Rounds, int computerRounds)
    {
        if (player1RoundsWonText != null)
        {
            player1RoundsWonText.text = $"Rounds Won: {player1Rounds}";
        }

        if (computerRoundsWonText != null)
        {
            computerRoundsWonText.text = $"Rounds Won: {computerRounds}";
        }
    }

    // ========== GAME OVER ==========
    private void ShowGameOver()
    {
        if (gameOverPanel == null) return;
        if (gameOverPanel.activeSelf) return; // Already showing

        gameOverPanel.SetActive(true);
        Vector3 targetPos = new Vector3(0f, 0f, 0f);
        gameOverPanel.transform.localScale = Vector3.zero;
        // LeanTween.scale(gameOverPanel, new Vector3(1f, 1f, 1f), 0.4f)
        //     .setEaseOutBack();

             LeanTween.scale(gameOverPanel, Vector3.one * 1.15f, 0.2f)
                .setDelay(0.2f)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() =>
                {
                    // Slight scale down (bounce back)
                    LeanTween.scale(gameOverPanel, Vector3.one * 0.9f, 0.18f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {
                            // Settle to original size
                            LeanTween.scale(gameOverPanel, Vector3.one, 0.15f)
                                .setEase(LeanTweenType.easeOutElastic);
                        });
                });

        // Determine winner by rounds won
        int player1Rounds = gameManager.GetPlayer1RoundsWon();
        int computerRounds = gameManager.GetComputerRoundsWon();

        string winnerMessage;
        Color winnerColor;
        Sprite backgroundResultSprite = null;
        if (player1Rounds > computerRounds)
        {
            winnerMessage = "YOU WON!";
            winnerColor = player1Color;
            backgroundResultSprite = winImage;
        }
        else if (computerRounds > player1Rounds)
        {
            winnerMessage = "YOU LOSE";
            winnerColor = computerColor;
            backgroundResultSprite = loseImage;
        }
        else
        {
            winnerMessage = "Draw";
            winnerColor = Color.yellow;
            backgroundResultSprite = drawImage;
        }
        if (gameResultImage != null && backgroundResultSprite != null)
        {
            gameResultImage.sprite = backgroundResultSprite;
            gameResultImage.enabled = true;

            RectTransform rt = gameResultImage.rectTransform;

            // Reset scale first (important when replaying scenes)
            rt.localScale = Vector3.zero;

            LeanTween.scale(rt, Vector3.one * 1.15f, 0.6f)
                .setDelay(0.2f)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() =>
                {
                    // Slight scale down (bounce back)
                    LeanTween.scale(rt, Vector3.one * 0.9f, 0.18f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {
                            // Settle to original size
                            LeanTween.scale(rt, Vector3.one, 0.15f)
                                .setEase(LeanTweenType.easeOutElastic);
                        });
                });
        }
        if (winnerText != null)
        {
            winnerText.text = winnerMessage == "Draw" ? "DRAW" : $"{winnerMessage}";
            winnerText.color = winnerColor;
        }

        if (gameScoreText != null)
        {
            gameScoreText.text = $"{ScoreManager.Instance.gameScore}";
        }
        if (capturedPotText != null)
        {
            capturedPotText.text = $"{ScoreManager.Instance.potCaptured}";
        }
        if (capturedStoneText != null)
        {
            capturedStoneText.text = $"{ScoreManager.Instance.stoneCaptured}";
        }
    }

    private void OnSettingsClicked()
    {

        Debug.Log("Opening Settings Menu clicked");
        if (settingsMenu != null)

        {
            settingsMenu.OpenSettings();
        }
    }

     private void OnPlayButtonClicked()
    {
         if (gameOverPanel == null) return;
        SoundManager.Instance.PlayButtonClick();
           
        if (gameOverPanel.activeSelf)
            {
                gameOverPanel.SetActive(false);
            }
  
    }

    private void OnBackMenuButtonClicked()
    {
        SoundManager.Instance.PlayButtonClick();
        SoundManager.Instance.StopBackgroundMusic();
        SceneManager.LoadScene(1);


    }
    // ========== BUTTON HANDLERS ==========
    private void OnRestartClicked()
    {
        // Reload the scene
        SoundManager.Instance.PlayButtonClick();
        SceneManager.LoadScene(
        SceneManager.GetActiveScene().name
        );
    }

    private void OnQuitClicked()
    {
        Debug.Log("OnQuitClicked called");
        SoundManager.Instance.PlayButtonClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
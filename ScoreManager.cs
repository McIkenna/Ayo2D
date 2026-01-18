using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private const string SCORE_KEY = "PLAYER_TOTAL_POINTS";
    public int TotalPoints { get; private set; }
    public int gameScore = 0;

    public int PointsEarned = 0;
    public int potCaptured = 0;
    public int stoneCaptured = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ------------------------
    //  PUBLIC FUNCTIONS
    // ------------------------

    public void AddPotCaptured()
    {
        potCaptured += 1;
        stoneCaptured += 4;
        AddPoints(10);
    }

     public void AddRemainingCaptured()
    {
        AddPoints(20);
    }

    public void AddRoundWon()
    {
        AddPoints(100);
    }

    public void AddRoundDraw()
    {
        AddPoints(50);
    }

    public void AddRoundLost()
    {
        AddPoints(0);
    }

    public void AddGameWon()
    {
        AddPoints(1000);
    }

    public void AddGameDraw()
    {
        AddPoints(500);
    }

    public void AddGameLost()
    {
        AddPoints(0);
    }

    // ------------------------
    //  CORE LOGIC
    // ------------------------

    private void AddPoints(int amount)
    {
        PointsEarned = amount;
        TotalPoints += amount;
        gameScore += amount;
        SaveScore();
    }

    private void SaveScore()
    {
        PlayerPrefs.SetInt(SCORE_KEY, TotalPoints);
        PlayerPrefs.Save();
    }

    private void LoadScore()
    {
        TotalPoints = PlayerPrefs.GetInt(SCORE_KEY, 0);
        gameScore = 0;
    }

    public void ResetGameScore()
    {
        potCaptured = 0;
        stoneCaptured = 0;
        gameScore = 0;
        
    }

    public void ResetScore()
    {
        TotalPoints = 0;
        SaveScore();
    }
}
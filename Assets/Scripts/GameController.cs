using System.Collections;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject endOfRoundPanel;
    [SerializeField] private DialogueController dialogueController; // 🔥 NOVO

    private EnemyMissileSpawner myEnemyMissileSpawner;
    
    public int score = 0;
    public int level = 1;
    public float enemyMissileSpeed = 5f;
    [SerializeField] private float enemyMissileSpeedMultiplier = 2f;

    public int playerMissilesLeft = 30;
    private int enemyMissilesThisRound = 20;
    private int enemyMissilesLeftInRound = 0;

    [SerializeField] private int missileEndOfRoundPoints = 5;
    [SerializeField] private int CityEndOfRoundPoints = 100;

    private int MissileDestroyedPoints = 25;
    
    [SerializeField] private TextMeshProUGUI myScoreText;
    [SerializeField] private TextMeshProUGUI myLevelText;
    [SerializeField] private TextMeshProUGUI myMissilesLeftText;
    
    [SerializeField] private TextMeshProUGUI leftOverMissileBonusText;
    [SerializeField] private TextMeshProUGUI leftOverCityBonusText;
    [SerializeField] private TextMeshProUGUI totalBonusText;
    [SerializeField] private TextMeshProUGUI countdownNumber;
    
    [SerializeField] private int maxLevels = 2;

    private bool isRoundOver = false;
    
    public enum GameState
    {
        Playing,
        EndRound,
        Dialogue
    }

    public GameState currentState;

    void Start()
    {
        currentState = GameState.Playing;
        
        myEnemyMissileSpawner = FindObjectOfType<EnemyMissileSpawner>();
        
        UpdateScoreText();
        UpdateLevelText();
        UpdateMissilesLeftText();
        
        StartRound();
    }

    void Update()
    {
        if (enemyMissilesLeftInRound <= 0 && !isRoundOver && currentState == GameState.Playing)
        {
            isRoundOver = true;
            StartCoroutine(EndOfRound());
        }
    }

    public void UpdateMissilesLeftText()
    {
        myMissilesLeftText.text = "Missiles Left: " + playerMissilesLeft;
    }

    public void UpdateScoreText()
    {
        myScoreText.text = "Score: " + score;
    }

    public void UpdateLevelText()
    {
        myLevelText.text = "Level: " + level;
    }

    public void AddMissileDestroyedPoints()
    {
        score += MissileDestroyedPoints;
        EnemyMissileDestroyed();
        UpdateScoreText();
    }

    public void EnemyMissileDestroyed()
    {
        enemyMissilesLeftInRound--;
    }

    private void StartRound()
    {
        myEnemyMissileSpawner.MissilesToSpawnThisRound = enemyMissilesThisRound;
        enemyMissilesLeftInRound = enemyMissilesThisRound;
        myEnemyMissileSpawner.StartRound();
    }

    public IEnumerator EndOfRound()
    {
        currentState = GameState.EndRound;

        yield return new WaitForSeconds(.5f);

        Time.timeScale = 0f;

        endOfRoundPanel.SetActive(true);

        int leftOverMissileBonus = playerMissilesLeft * missileEndOfRoundPoints;

        GameObject[] cities = GameObject.FindGameObjectsWithTag("Defenders");
        int leftOverCityBonus = cities.Length * CityEndOfRoundPoints;

        int totalBonus = leftOverCityBonus + leftOverMissileBonus;

        leftOverMissileBonusText.text = "Left Over Missile Bonus: " + leftOverMissileBonus;
        leftOverCityBonusText.text = "Left Over City Bonus: " + leftOverCityBonus;
        totalBonusText.text = "Total Bonus: " + totalBonus;

        score += totalBonus;
        UpdateScoreText();

        yield return new WaitForSecondsRealtime(1f);
        countdownNumber.text = "3";

        yield return new WaitForSecondsRealtime(1f);
        countdownNumber.text = "2";

        yield return new WaitForSecondsRealtime(1f);
        countdownNumber.text = "1";

        if (level >= maxLevels)
        {
            EndGame();
        }
        else
        {
            dialogueController.StartDialogue();
        }
    }

    
    void EndGame()
    {
        Debug.Log("FIM DE JOGO");

        Time.timeScale = 0f;

        // opcional: mostrar um painel de fim
    }
    
    
    // 🔥 ESSENCIAL
    public void ContinueGameFromDialogue()
    {
        Time.timeScale = 1f;

        endOfRoundPanel.SetActive(false); 

        isRoundOver = false;

        level++;
        playerMissilesLeft = 30;

        enemyMissileSpeed *= enemyMissileSpeedMultiplier;

        UpdateLevelText();
        UpdateMissilesLeftText();

        currentState = GameState.Playing;

        StartRound();
    } 
}
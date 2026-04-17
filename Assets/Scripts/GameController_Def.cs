using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameController_Def : MonoBehaviour
{
    EnemyMissileSpawner_Def myEnemyMissileSpawner;

    
    public int score = 0;  
    public int level = 1;
    public int playerMissilesLeft = 30;
    private int enemyMissilesThisRound = 20;
    private int enemyMissilesLeftInRound = 0;

    [SerializeField] private TextMeshProUGUI myScoreText;
    [SerializeField] private TextMeshProUGUI myLevelText;
    [SerializeField] private TextMeshProUGUI myMissilesLeftText;


    private int missileDestroyedPoints = 25;    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myEnemyMissileSpawner = GameObject.FindObjectOfType<EnemyMissileSpawner_Def>();
        
        UpdateScoreText();
        UpdateLevelText();
        UpdateMissilesLeftText();

        StartRound();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyMissilesLeftInRound <= 0)
        {
            Debug.Log("Round Over!");
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
        score += missileDestroyedPoints;
        EnemyMissileDestroyed();
        UpdateScoreText();
    }

    public void EnemyMissileDestroyed()
    {
        enemyMissilesLeftInRound--;
    }

    private void StartRound()
    {
        myEnemyMissileSpawner.missilesToSpawnThisRound = enemyMissilesThisRound;
        enemyMissilesLeftInRound = enemyMissilesThisRound;
        myEnemyMissileSpawner.StartRound();

    }
}

using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private EnemyMissileSpawner myEnemyMissileSpawner;

    public float enemyMissileSpeed = 5f;

    [SerializeField] private float spawnDelay = 1.5f;
    [SerializeField] private float spawnAcceleration = 0.98f; // vai diminuindo o tempo

    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver = false;

    void Start()
    {
        myEnemyMissileSpawner = FindObjectOfType<EnemyMissileSpawner>();

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (!isGameOver)
        {
            SpawnMissile();

            yield return new WaitForSeconds(spawnDelay);

            // dificuldade aumenta com o tempo
            spawnDelay *= spawnAcceleration;
            spawnDelay = Mathf.Clamp(spawnDelay, 0.3f, 2f);
        }
    }

    void SpawnMissile()
    {
        myEnemyMissileSpawner.SpawnSingleMissile(enemyMissileSpeed);
    }

    // 🔥 CHAMAR ISSO QUANDO UMA CIDADE MORRER
    public void CheckGameOver()
    {
        GameObject[] cities = GameObject.FindGameObjectsWithTag("Defenders");

        if (cities.Length == 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        isGameOver = true;

        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("GAME OVER");
    }
}
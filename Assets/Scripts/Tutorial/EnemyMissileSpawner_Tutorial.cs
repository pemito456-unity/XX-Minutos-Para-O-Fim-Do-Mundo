using System.Collections;
using UnityEngine;

public class EnemyMissileSpawner_Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float Ypadding = 0.5f;

    [Header("Área de Exclusão (Interface)")]
    [SerializeField] private float excludeXMin = -8.36f;
    [SerializeField] private float excludeXMax = -3.83f;
    
    [Header("Margem de Segurança")]
    [SerializeField] private float safeMargin = 0.2f;

    [Header("Configuração de Spawn (Tutorial)")]
    [SerializeField] private float spawnY = 3f;
    
    [Header("Velocidade dos Meteoros")]
    [SerializeField] private float missileSpeed = 2f;

    private float minX, maxX;
    public float delayBetweenMissiles = 1.5f;

    private bool isSpawning;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

        if (missilePrefab == null)
            Debug.LogError("EnemyMissileSpawner_Tutorial: missilePrefab não está atribuído!");
    }

    public void SetMissileSpeed(float speed)
    {
        missileSpeed = speed;
    }

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnMissilesLoop());
        Debug.Log("Spawn de meteoros do tutorial iniciado!");
    }

    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log("Spawn de meteoros do tutorial parado!");
    }

    private IEnumerator SpawnMissilesLoop()
    {
        SpawnOneMissile();

        while (isSpawning)
        {
            yield return new WaitForSeconds(delayBetweenMissiles);
            if (!isSpawning)
                break;

            SpawnOneMissile();
        }
    }

    private void SpawnOneMissile()
    {
        float spawnX = GetRandomXExcludingInterface();

        if (IsXInForbiddenArea(spawnX))
        {
            Debug.LogWarning($"X={spawnX} está na área proibida! Recusando spawn.");
            return;
        }

        Vector3 spawnPosition = new Vector3(spawnX, spawnY + Ypadding, 0);

        if (missilePrefab == null)
            return;

        GameObject newMissile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);

        EnemyMissile_Tutorial tutorialMissile = newMissile.GetComponent<EnemyMissile_Tutorial>();
        if (tutorialMissile != null)
            tutorialMissile.SetSpeed(missileSpeed);

        Debug.Log($"Meteoro tutorial spawnado em {spawnPosition}");
    }

    private float GetRandomXExcludingInterface()
    {
        float leftAreaWidth = (excludeXMin - safeMargin) - minX;
        float rightAreaWidth = maxX - (excludeXMax + safeMargin);
        float totalFreeWidth = leftAreaWidth + rightAreaWidth;

        if (totalFreeWidth <= 0)
            return Random.Range(minX, maxX);

        if (Random.Range(0, totalFreeWidth) < leftAreaWidth)
            return Random.Range(minX, excludeXMin - safeMargin);

        return Random.Range(excludeXMax + safeMargin, maxX);
    }
    
    private bool IsXInForbiddenArea(float x)
    {
        float forbiddenStart = excludeXMin - safeMargin;
        float forbiddenEnd = excludeXMax + safeMargin;
        return x >= forbiddenStart && x <= forbiddenEnd;
    }
}

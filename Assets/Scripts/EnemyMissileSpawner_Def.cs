using System.Collections;
using UnityEngine;

public class EnemyMissileSpawner_Def : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float Ypadding = 0.5f;

    [Header("Área de Exclusão (Interface)")]
    [SerializeField] private float excludeXMin = -8.36f;
    [SerializeField] private float excludeXMax = -3.83f;
    
    [Header("Margem de Segurança")]
    [SerializeField] private float safeMargin = 0.2f;

    [Header("Configuração de Spawn")]
    [SerializeField] private float spawnY = -2f;
    
    [Header("Dificuldade Progressiva")]
    [SerializeField] private float initialSpawnDelay = 3.5f;
    [SerializeField] private float finalSpawnDelay = 0.8f;
    [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private float minX, maxX;
    private float currentDelay;
    private GameController_Def gameController;
    private int maxInvestigationSteps = 3;

    void Awake()
    {
        Debug.Log("Awake: Iniciando Spawner");
        
        minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        
        Debug.Log($"Limites da tela: minX={minX}, maxX={maxX}");
        Debug.Log($"Área excluída: {excludeXMin} até {excludeXMax}");
        
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        

        
        currentDelay = initialSpawnDelay;
        StartCoroutine(SpawnMissilesLoop());
        StartCoroutine(UpdateDifficulty());
    }

    private IEnumerator UpdateDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (gameController != null)
            {
                int currentProgress = GetCurrentInvestigationProgress();
                float t = Mathf.Clamp01((float)currentProgress / maxInvestigationSteps);
                float curveValue = difficultyCurve.Evaluate(t);
                currentDelay = Mathf.Lerp(initialSpawnDelay, finalSpawnDelay, curveValue);
                
                Debug.Log($"Dificuldade: Progresso={currentProgress}/{maxInvestigationSteps}, Delay={currentDelay:F2}s");
            }
        }
    }
    
    private int GetCurrentInvestigationProgress()
    {
        System.Type type = gameController.GetType();
        var field = type.GetField("currentInvestigationProgress", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (int)field.GetValue(gameController);
        }
        return 0;
    }

    private IEnumerator SpawnMissilesLoop()
    {
        while (true) 
        {
            yield return new WaitForSeconds(currentDelay);
            
            if (gameController != null && gameController.currentState == GameController_Def.GameState.Gameplay)
            {
                float spawnX = GetRandomXExcludingInterface();
                
                if (IsXInForbiddenArea(spawnX))
                {
                    Debug.LogWarning($"X={spawnX} está na área proibida! Recusando spawn.");
                    continue; 
                }
                
                Vector3 spawnPosition = new Vector3(spawnX, spawnY + Ypadding, 0);

                if (missilePrefab != null) 
                {
                    GameObject newMissile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
                    Debug.Log($"Míssil spawnado em X: {spawnX} ✓ Delay atual: {currentDelay:F2}s");
                }
            }
        }
    }

    private float GetRandomXExcludingInterface()
    {
        float leftAreaWidth = (excludeXMin - safeMargin) - minX;
        float rightAreaWidth = maxX - (excludeXMax + safeMargin);
        float totalFreeWidth = leftAreaWidth + rightAreaWidth;

        if (totalFreeWidth <= 0) return Random.Range(minX, maxX);

        if (Random.Range(0, totalFreeWidth) < leftAreaWidth)
            return Random.Range(minX, excludeXMin - safeMargin);
        else
            return Random.Range(excludeXMax + safeMargin, maxX);
    }
    
    private bool IsXInForbiddenArea(float x)
    {
        float forbiddenStart = excludeXMin - safeMargin;
        float forbiddenEnd = excludeXMax + safeMargin;
        
        return (x >= forbiddenStart && x <= forbiddenEnd);
    }
}
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

    private float minX, maxX;
    public float delayBetweenMissiles = 1.5f;
    private GameController_Def gameController;

    void Awake()
    {
        Debug.Log("Awake: Iniciando Spawner");
        
        minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        
        Debug.Log($"Limites da tela: minX={minX}, maxX={maxX}");
        Debug.Log($"Área excluída: {excludeXMin} até {excludeXMax}");
        
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        
        if (missilePrefab == null)
        {
            Debug.LogError("ERRO: missilePrefab não está atribuído no Inspector!");
        }
        
        StartCoroutine(SpawnMissilesLoop());
    }

    private IEnumerator SpawnMissilesLoop()
    {
        while (true) 
        {
            yield return new WaitForSeconds(delayBetweenMissiles);
            
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
                    Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
                    Debug.Log($"Míssil spawnado em X: {spawnX} ✓");
                }
            }
        }
    }

    private float GetRandomXExcludingInterface()
    {
        float leftAreaWidth = (excludeXMin - safeMargin) - minX;
        float rightAreaWidth = maxX - (excludeXMax + safeMargin);
        float totalFreeWidth = leftAreaWidth + rightAreaWidth;

        Debug.Log($"LeftArea: {leftAreaWidth}, RightArea: {rightAreaWidth}, Total: {totalFreeWidth}");

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
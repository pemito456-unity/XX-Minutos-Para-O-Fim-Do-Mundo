using System.Collections;
using UnityEngine;

public class EnemyMissileSpawner_Def : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float Ypadding = 0.5f;

    [Header("Área de Exclusão (Interface)")]
    [SerializeField] private float excludeXMin = -8.36f;
    [SerializeField] private float excludeXMax = -3.35f;

    private float minX, maxX, yValue;
    public float delayBetweenMissiles = 1.5f;
    private GameController_Def gameController;

    void Awake()
    {
        Debug.Log("Awake: Iniciando Spawner");
        
        minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;
        maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x;
        yValue = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        
        Debug.Log($"minX: {minX}, maxX: {maxX}, yValue: {yValue}");
        
        // TUDO que estava no Start vai para o Awake
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        Debug.Log($"GameController encontrado? {gameController != null}");
        
        if (missilePrefab == null)
        {
            Debug.LogError("ERRO: missilePrefab não está atribuído no Inspector!");
        }
        else
        {
            Debug.Log("MissilePrefab atribuído corretamente");
        }
        
        // Inicia a corrotina IMEDIATAMENTE
        StartCoroutine(SpawnMissilesLoop());
    }

    void Start()
    {
        Debug.Log("Start do Spawner rodou (só pra confirmar que o objeto está ativo)");
    }

    private IEnumerator SpawnMissilesLoop()
    {
        Debug.Log("Corrotina de SPAWN iniciada!");
        
        while (true) 
        {
            Debug.Log($"Esperando {delayBetweenMissiles} segundos...");
            yield return new WaitForSeconds(delayBetweenMissiles);
            
            Debug.Log($"Verificando estado: gameController null? {gameController == null}, estado: {gameController?.currentState}");
            
            if (gameController != null && gameController.currentState == GameController_Def.GameState.Gameplay)
            {
                float spawnX = GetRandomXExcludingInterface();
                Vector3 spawnPosition = new Vector3(spawnX, yValue + Ypadding, 0);

                if (missilePrefab != null) 
                {
                    GameObject newMissile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
                    Debug.Log($"MÍSSIL SPAWNADO em X: {spawnX}");
                }
            }
            else
            {
                Debug.Log("Condição não atendida para spawnar");
            }
        }
    }

    private float GetRandomXExcludingInterface()
    {
        float leftAreaWidth = excludeXMin - minX;
        float rightAreaWidth = maxX - excludeXMax;
        float totalFreeWidth = leftAreaWidth + rightAreaWidth;

        if (totalFreeWidth <= 0) return Random.Range(minX, maxX);

        if (Random.Range(0, totalFreeWidth) < leftAreaWidth)
            return Random.Range(minX, excludeXMin);
        else
            return Random.Range(excludeXMax, maxX);
    }
}
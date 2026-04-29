using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameController_Def : MonoBehaviour
{
    public enum GameState { Gameplay, Dialogue, GameOver, Victory }
    public GameState currentState;

    [Header("Sistema de Pressão")]
    [SerializeField] private float currentPressure = 0f;
    [SerializeField] private float maxPressure = 100f;
    
    [Header("Progressão da História (Vitória)")]
    [SerializeField] private int requiredInvestigationProgress = 3; // Quantos diálogos do cientista precisa
    private int currentInvestigationProgress = 0;
    
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    
    // Lista de defensores vivos
    private List<GameObject> activeDefenders = new List<GameObject>();
    private int totalDefendersCount = 0;

    void Start()
    {
        Time.timeScale = 1f;
        currentState = GameState.Gameplay;
        currentPressure = 0f;
        currentInvestigationProgress = 0;
        
        if(gameOverPanel) gameOverPanel.SetActive(false);
        if(victoryPanel) victoryPanel.SetActive(false);
        
        RegisterAllDefenders();
        Debug.Log("GameController iniciado. Vitória requer progresso: " + requiredInvestigationProgress);
    }

    void Update()
    {
        if (currentState == GameState.Gameplay)
        {
            CheckLoseConditions();
        }
    }
    
    private void RegisterAllDefenders()
    {
        GameObject[] defenders = GameObject.FindGameObjectsWithTag("Defenders");
        activeDefenders.Clear();
        activeDefenders.AddRange(defenders);
        totalDefendersCount = activeDefenders.Count;
        Debug.Log($"Registrados {totalDefendersCount} defensores");
    }
    
    public void OnDefenderDestroyed(GameObject destroyedDefender)
{
    Debug.Log($"OnDefenderDestroyed chamado para: {destroyedDefender.name}");
    
    if (activeDefenders.Contains(destroyedDefender))
    {
        activeDefenders.Remove(destroyedDefender);
        Debug.Log($"Defensor removido. Restam: {activeDefenders.Count}/{totalDefendersCount}");
        
        int destroyedCount = totalDefendersCount - activeDefenders.Count;
        float destructionPercent = (float)destroyedCount / totalDefendersCount;
        currentPressure = destructionPercent * maxPressure;
        currentPressure = Mathf.Clamp(currentPressure, 0, maxPressure);
        
        Debug.Log($"PRESSURE ATUALIZADA: {currentPressure}/{maxPressure} ({destructionPercent * 100}% destruído)");
    }
    else
    {
        Debug.LogWarning($"Destroyed {destroyedDefender.name} não estava na lista de defensores!");
    }
}
    
    public void OnMissileIntercepted()
    {
        Debug.Log("Míssil abatido!");
    }
    
    public void OnMissileHitCity()
    {
        Debug.Log("Míssil ACERTOU a cidade!");
    }
    
    // Chamado pelos diálogos do cientista - PROGRIDE A VITÓRIA
    public void AdvanceInvestigation()
    {
        currentInvestigationProgress++;
        Debug.Log($"Investigação progrediu: {currentInvestigationProgress}/{requiredInvestigationProgress}");
        
        if (currentInvestigationProgress >= requiredInvestigationProgress)
        {
            TriggerVictory();
        }
    }
    
    // Chamado por diálogos (coronel/secretário) - afeta pressão amarela
    public void ModifyPressureByDialogue(float amount)
    {
        currentPressure += amount;
        currentPressure = Mathf.Clamp(currentPressure, 0, maxPressure);
        Debug.Log($"Diálogo alterou pressão em {amount}. Pressão atual: {currentPressure}");
    }
    
    private void CheckLoseConditions()
    {
        if (currentPressure >= maxPressure)
        {
            Debug.Log($"GAME OVER: Pressão {currentPressure} >= {maxPressure}");
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;
        Time.timeScale = 0f; 
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Debug.Log("GAME OVER - Pressão máxima atingida!");
    }

    public void TriggerVictory()
    {
        if (currentState == GameState.Victory) return;
        currentState = GameState.Victory;
        Time.timeScale = 0f;
        if (victoryPanel) victoryPanel.SetActive(true);
        Debug.Log("VITÓRIA - Investigação completa! Meteoros identificados, guerra evitada!");
    }
    
    public float GetPressurePercent() => currentPressure / maxPressure;
}
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController_Def : MonoBehaviour
{
    public enum GameState { Gameplay, Dialogue, GameOver, Victory }
    public GameState currentState;

    [Header("Sistema de Pressão")]
    [SerializeField] private float maxPressure = 100f;
    private float redPressure = 0f; 
    private float yellowPressure = 0f; 
    
    [Header("UI de Pressão")]
    [SerializeField] private TextMeshProUGUI redPressureText;
    [SerializeField] private TextMeshProUGUI yellowPressureText;
    [SerializeField] private GameObject pressurePanel;
    
    [Header("Progressão da História")]
    [SerializeField] private int requiredInvestigationProgress = 3;
    private int currentInvestigationProgress = 0;
    
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject sceneImages;
    
    [Header("Configuração de Cenas")]
    [SerializeField] private string mainGameScene = "MainGame";
    [SerializeField] private string mainMenuScene = "MainMenu";
    
    public float GetRedPressurePercent() => redPressure / maxPressure;
    public float GetYellowPressurePercent() => yellowPressure / maxPressure;
    public float GetTotalPressurePercent() => (redPressure + yellowPressure) / maxPressure;
    
    public float GetPressurePercent() => GetTotalPressurePercent();
    
    public int GetCurrentInvestigationProgress() => currentInvestigationProgress;
    public int GetRequiredInvestigationProgress() => requiredInvestigationProgress;
    
    private List<GameObject> activeDefenders = new List<GameObject>();
    private int totalDefendersCount = 0;

    void Start()
    {
        Time.timeScale = 1f;
        currentState = GameState.Gameplay;
        redPressure = 0f;
        yellowPressure = 0f;
        currentInvestigationProgress = 0;
        
        if(gameOverPanel) gameOverPanel.SetActive(false);
        if(victoryPanel) victoryPanel.SetActive(false);
        
        RegisterAllDefenders();
        UpdatePressureUI();
        Debug.Log($"GameController iniciado. Pressão máx: {maxPressure}");
    }

    void Update()
    {
        if (currentState == GameState.Gameplay)
        {
            CheckLoseConditions();
        }
    }
    
    private void UpdatePressureUI()
    {
        if (redPressureText != null)
        {
            redPressureText.text = $"DESTRUIÇÃO: {redPressure:F0}/{maxPressure}";
        }
        
        if (yellowPressureText != null)
        {
            yellowPressureText.text = $"POLÍTICA: {yellowPressure:F0}/{maxPressure}";
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

    public void AddRedPressure(float amount)
    {
        redPressure = Mathf.Clamp(redPressure + amount, 0, maxPressure);
        UpdatePressureUI();
        Debug.Log($" PRESSÃO VERMELHA: +{amount} → {redPressure:F1}/{maxPressure}");
        CheckLoseConditions();
    }
    
    public void AddYellowPressure(float amount)
    {
        yellowPressure = Mathf.Clamp(yellowPressure + amount, 0, maxPressure);
        UpdatePressureUI();
        string arrow = amount > 0 ? "↑" : "↓";
        Debug.Log($" PRESSÃO AMARELA: {arrow}{Mathf.Abs(amount)} → {yellowPressure:F1}/{maxPressure}");
        CheckLoseConditions();
    }

    public void ModifyPressureByDialogue(float amount)
    {
        AddYellowPressure(amount);
    }
    
    public void OnDefenderDestroyed(GameObject destroyedDefender)
    {
        if (activeDefenders.Contains(destroyedDefender))
        {
            activeDefenders.Remove(destroyedDefender);
            
            int destroyedCount = totalDefendersCount - activeDefenders.Count;
            float destructionPercent = (float)destroyedCount / totalDefendersCount;
            
            redPressure = destructionPercent * maxPressure;
            redPressure = Mathf.Clamp(redPressure, 0, maxPressure);
            UpdatePressureUI();
            
            Debug.Log($" Prédio destruído! {destroyedCount}/{totalDefendersCount}. Pressão Vermelha: {redPressure:F1}/{maxPressure}");
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
    
    public void AdvanceInvestigation()
    {
        currentInvestigationProgress++;
        Debug.Log($"Investigação: {currentInvestigationProgress}/{requiredInvestigationProgress}");
        
        if (currentInvestigationProgress >= requiredInvestigationProgress)
        {
            TriggerVictory();
        }
    }
    
    private void CheckLoseConditions()
    {
        float totalPressure = redPressure + yellowPressure;
        if (totalPressure >= maxPressure)
        {
            Debug.Log($"GAME OVER: Pressão total {totalPressure:F1} >= {maxPressure}");
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        
        DestroyAllEnemyMissiles();
        
        if (sceneImages != null) sceneImages.SetActive(false);
        if (pressurePanel != null) pressurePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        Debug.Log(" GAME OVER!");
    }
    
    private void DestroyAllEnemyMissiles()
    {
        GameObject[] enemyMissiles = GameObject.FindGameObjectsWithTag("EnemyMissile");
        foreach (GameObject missile in enemyMissiles)
        {
            Destroy(missile);
        }
        Debug.Log($"Destruídos {enemyMissiles.Length} mísseis inimigos");
    }
    
    public void TryAgain()
    {
        Debug.Log("Tentando novamente...");
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(mainGameScene))
        {
            SceneManager.LoadScene(mainGameScene);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void QuitToMenu()
    {
        Debug.Log("Voltando ao menu principal...");
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            Debug.LogError("Nome da cena do menu não configurado!");
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    public void TriggerVictory()
    {
        if (currentState == GameState.Victory) return;
        currentState = GameState.Victory;
        Time.timeScale = 0f;
        
        DestroyAllEnemyMissiles();
        
        if (sceneImages != null) sceneImages.SetActive(false);
        if (pressurePanel != null) pressurePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        
        Debug.Log(" VITÓRIA!");
    }
}
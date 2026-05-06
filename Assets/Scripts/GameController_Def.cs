using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; // 🔴 Adicionado para gerenciar cenas

public class GameController_Def : MonoBehaviour
{
    public enum GameState { Gameplay, Dialogue, GameOver, Victory }
    public GameState currentState;

    [Header("Sistema de Pressão")]
    [SerializeField] private float currentPressure = 0f;
    [SerializeField] private float maxPressure = 100f;
    
    [Header("Progressão da História (Vitória)")]
    [SerializeField] private int requiredInvestigationProgress = 3;
    private int currentInvestigationProgress = 0;
    
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject sceneImages; // 🔴 Referência ao SceneImages
    
    [Header("Configuração de Cenas")]
    [SerializeField] private string mainGameScene = "MainGame";
    [SerializeField] private string mainMenuScene = "MainMenu";
    
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
    
    public void AdvanceInvestigation()
    {
        currentInvestigationProgress++;
        Debug.Log($"Investigação progrediu: {currentInvestigationProgress}/{requiredInvestigationProgress}");
        
        if (currentInvestigationProgress >= requiredInvestigationProgress)
        {
            TriggerVictory();
        }
    }
    
    public void ModifyPressureByDialogue(float amount)
    {
        currentPressure += amount;
        currentPressure = Mathf.Clamp(currentPressure, 0, maxPressure);
        Debug.Log($"Diálogo alterou pressão em {amount}. Pressão atual: {currentPressure}");
        
        // Verifica game over imediatamente após a mudança
        if (currentPressure >= maxPressure && currentState == GameState.Gameplay)
        {
            TriggerGameOver();
        }
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
        
        // 🔴 Desativa o SceneImages
        if (sceneImages != null)
        {
            sceneImages.SetActive(false);
            Debug.Log("SceneImages desativado no Game Over");
        }
        
        // Ativa o painel de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOverPanel ativado");
        }
        
        Debug.Log("GAME OVER - Pressão máxima atingida!");
    }

    public void TriggerVictory()
    {
        if (currentState == GameState.Victory) return;
        currentState = GameState.Victory;
        Time.timeScale = 0f;
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        
        Debug.Log("VITÓRIA - Investigação completa! Meteoros identificados, guerra evitada!");
    }
    
    public float GetPressurePercent() => currentPressure / maxPressure;
    
    // 🔴 MÉTODOS PARA REINICIAR/VOLTAR AO MENU
    public void ReiniciarJogo()
    {
        Debug.Log("Reiniciando jogo...");
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(mainGameScene))
        {
            SceneManager.LoadScene(mainGameScene);
        }
        else
        {
            Debug.LogError("O nome da cena principal não foi definido no Inspector!");
            // Fallback: recarrega a cena atual
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void VoltarAoMenu()
    {
        Debug.Log("Voltando ao menu principal...");
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            Debug.LogError("O nome da cena do menu não foi definido no Inspector!");
        }
    }
    
    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}
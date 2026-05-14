using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject termsPanel;
    
    [Header("GameObjects para Desativar")]
    [SerializeField] private GameObject sceneImages;
    
    [Header("Configuração de Cenas")]
    [SerializeField] private string mainGameScene = "MainGame";
    [SerializeField] private string mainMenuScene = "MainMenu";
    
    private bool isPaused = false;
    private GameController_Def gameController;
    
    void Start()
    {
        gameController = FindAnyObjectByType<GameController_Def>();
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (termsPanel != null)
            termsPanel.SetActive(false);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (termsPanel != null && termsPanel.activeSelf)
            {
                HideTerms();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    private void DestroyAllEnemyMissiles()
    {
        GameObject[] enemyMissiles = GameObject.FindGameObjectsWithTag("EnemyMissile");
        foreach (GameObject missile in enemyMissiles)
        {
            Destroy(missile);
        }
        Debug.Log($"Destruídos {enemyMissiles.Length} mísseis inimigos ao pausar");
    }
    
    public void PauseGame()
    {
        if (gameController != null && gameController.currentState == GameController_Def.GameState.Gameplay)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            DestroyAllEnemyMissiles();
            
            if (sceneImages != null)
                sceneImages.SetActive(false);
            
            if (pausePanel != null)
                pausePanel.SetActive(true);
            
            if (gameController != null)
                gameController.currentState = GameController_Def.GameState.Dialogue;
            
            Debug.Log("Jogo pausado");
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (sceneImages != null)
            sceneImages.SetActive(true);
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (termsPanel != null)
            termsPanel.SetActive(false);
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;
        
        Debug.Log("Jogo retomado");
    }
    
    public void ShowTerms()
    {
        Debug.Log("Mostrando glossário...");
        
        if (termsPanel != null)
        {
            termsPanel.SetActive(true);
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
    
    public void HideTerms()
    {
        Debug.Log("Fechando glossário...");
        
        if (termsPanel != null)
        {
            termsPanel.SetActive(false);
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("Reiniciando jogo...");
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
    
    public void BackToMenu()
    {
        Debug.Log("Voltando ao menu...");
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
    
    public bool IsPaused() => isPaused;
}
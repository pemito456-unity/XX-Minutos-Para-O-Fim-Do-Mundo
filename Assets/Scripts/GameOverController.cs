using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("Configuração de Cenas")]
    [Tooltip("Digite o nome da cena principal para reiniciar")]
    public string mainGameScene = "MainGame";
    
    [Tooltip("Digite o nome da cena do menu principal")]
    public string mainMenuScene = "MainMenu";

    [Header("Painéis do Game Over")]
    public GameObject gameOverPanel;
    
    [Header("GameObjects para Desativar")]
    public GameObject sceneImages; // Referência ao GameObject SceneImages
    
    [Header("Botões (opcional)")]
    public GameObject restartButton;
    public GameObject menuButton;
    public GameObject quitButton;

    void Start()
    {
        // 🔴 CORREÇÃO: NÃO chama MostrarGameOver automaticamente!
        // O GameOver só deve ser mostrado quando chamado pelo GameController
        // MostrarGameOver(); ← REMOVA ou COMENTE esta linha
        
        // Garante que o painel de Game Over começa desativado
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Garante que o SceneImages começa ATIVADO
        if (sceneImages != null)
        {
            sceneImages.SetActive(true);
            Debug.Log("SceneImages ativado no Start do GameOverController");
        }
        
        Debug.Log("GameOverController inicializado. Aguardando TriggerGameOver...");
    }

    public void MostrarGameOver()
    {
        // Desativa o SceneImages
        if (sceneImages != null)
        {
            sceneImages.SetActive(false);
            Debug.Log("SceneImages desativado - Game Over");
        }
        
        // Desativa todos os outros painéis (se houver)
        DesativarTodosOsPaineis();
        
        // Ativa o painel de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOverPanel ativado");
        }
        else
        {
            Debug.LogError("GameOverPanel não está atribuído no Inspector!");
        }
        
        // Pausa o jogo
        Time.timeScale = 0f;
    }

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

    private void DesativarTodosOsPaineis()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        // Adicione outros painéis aqui se necessário
    }
}
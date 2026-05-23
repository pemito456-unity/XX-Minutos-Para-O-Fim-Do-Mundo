using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject termsPanel;
    [SerializeField] private GameObject pauseMenuContent;

    [Header("GameObjects para Desativar")]
    [SerializeField] private GameObject sceneImages;

    [Header("Configuração de Cenas")]
    [SerializeField] private string mainGameScene = "Principal_Missil";
    [SerializeField] private string mainMenuScene = "Menu_Principal";

    private bool isPaused;
    private bool isShowingTerms;
    private GameController_Def gameController;

    void Start()
    {
        gameController = FindAnyObjectByType<GameController_Def>();
        ResolverReferencias();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (termsPanel != null)
            termsPanel.SetActive(false);
    }

    void ResolverReferencias()
    {
        if (pauseMenuContent == null && pausePanel != null)
        {
            Transform content = pausePanel.transform.Find("PauseContent");
            if (content != null)
                pauseMenuContent = content.gameObject;
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (isShowingTerms)
        {
            HideTerms();
            return;
        }

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void DestroyAllEnemyMissiles()
    {
        GameObject[] enemyMissiles = GameObject.FindGameObjectsWithTag("EnemyMissile");
        foreach (GameObject missile in enemyMissiles)
            Destroy(missile);
    }

    public void PauseGame()
    {
        if (gameController == null || gameController.currentState != GameController_Def.GameState.Gameplay)
            return;

        isPaused = true;
        isShowingTerms = false;
        Time.timeScale = 0f;

        DestroyAllEnemyMissiles();

        if (sceneImages != null)
            sceneImages.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseMenuContent != null)
            pauseMenuContent.SetActive(true);

        if (termsPanel != null)
            termsPanel.SetActive(false);

        gameController.currentState = GameController_Def.GameState.Dialogue;
    }

    public void ResumeGame()
    {
        isPaused = false;
        isShowingTerms = false;
        Time.timeScale = 1f;

        if (sceneImages != null)
            sceneImages.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (termsPanel != null)
            termsPanel.SetActive(false);

        if (pauseMenuContent != null)
            pauseMenuContent.SetActive(true);

        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;
    }

    public void ShowTerms()
    {
        if (!isPaused)
            return;

        isShowingTerms = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseMenuContent != null)
            pauseMenuContent.SetActive(false);

        if (termsPanel != null)
            termsPanel.SetActive(true);
    }

    public void HideTerms()
    {
        if (!isPaused)
            return;

        isShowingTerms = false;

        if (termsPanel != null)
            termsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseMenuContent != null)
            pauseMenuContent.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainGameScene))
            SceneManager.LoadScene(mainGameScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
            Debug.LogError("Nome da cena do menu não configurado!");
    }

    public bool IsPaused() => isPaused;
    public bool IsShowingTerms() => isShowingTerms;
}

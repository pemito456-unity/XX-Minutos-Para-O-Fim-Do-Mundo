using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class TutorialController : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private int meteorosNaOnda = 10;
    [SerializeField] private float tempoEntreMeteoros = 1.5f;
    [SerializeField] private float velocidadeMeteoros = 2f;
    
    [Header("Referências")]
    [SerializeField] private EnemyMissileSpawner_Def spawner;
    [SerializeField] private GameController_Def gameController;
    [SerializeField] private DialogueUITutorial dialogueManager;
    
    [Header("UI do Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button continuarButton;
    
    [Header("Diálogos do Tutorial")]
    [SerializeField] private DialogueData dialogoIntroducao;
    [SerializeField] private DialogueData dialogoPressao;
    [SerializeField] private DialogueData dialogoFinal;
    
    private int meteorosDestruidos = 0;
    private int meteorosAcertaram = 0;
    private bool tutorialEmAndamento = true;
    private bool ondaEmAndamento = false;
    private List<GameObject> meteorosAtivos = new List<GameObject>();
    
    void Start()
    {
        ConfigurarModoTutorial();
        StartCoroutine(RunTutorial());
    }
    
    void ConfigurarModoTutorial()
    {
        if (spawner != null)
            spawner.enabled = false;
        
        if (gameController != null)
        {
            gameController.currentState = GameController_Def.GameState.Dialogue;
        }
        
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Time.timeScale = 0f;
    }
    
    IEnumerator RunTutorial()
    {
        yield return StartCoroutine(MostrarDialogo(dialogoIntroducao));
        
        yield return StartCoroutine(ExplicarMeteoros());
        
        yield return StartCoroutine(OndaDeMeteoros());
        
        yield return StartCoroutine(ResultadoOnda());
        
        yield return StartCoroutine(MostrarDialogo(dialogoPressao));
        
        yield return StartCoroutine(MostrarDialogo(dialogoFinal));
        
        FinalizarTutorial();
    }
    
    IEnumerator MostrarDialogo(DialogueData dialogo)
    {
        if (dialogo == null) yield break;
        
        bool dialogoCompleto = false;
        
        dialogueManager.ShowDialogue(dialogo, () => {
            dialogoCompleto = true;
        });
        
        while (!dialogoCompleto)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    IEnumerator ExplicarMeteoros()
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = "Clique nos meteoros para destruí-los!\n\nProteja Washington dos impactos.";
        
        continuarButton.onClick.RemoveAllListeners();
        continuarButton.onClick.AddListener(() => {
            tutorialPanel.SetActive(false);
            Time.timeScale = 1f;
        });
        
        Time.timeScale = 0f;
        
        while (tutorialPanel.activeSelf)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator OndaDeMeteoros()
    {
        ondaEmAndamento = true;
        meteorosDestruidos = 0;
        meteorosAcertaram = 0;
        
        StartCoroutine(MonitorarOnda());
        
        for (int i = 0; i < meteorosNaOnda; i++)
        {
            yield return new WaitForSeconds(tempoEntreMeteoros);
            SpawnMeteoroManual();
        }
        
        while (ondaEmAndamento)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    void SpawnMeteoroManual()
    {
        float randomX = Random.Range(-7f, 7f);
        Vector3 posicao = new Vector3(randomX, 6f, 0);
        
        GameObject meteoro = new GameObject("TutorialMeteoro");
        meteoro.tag = "EnemyMissile";
        meteorosAtivos.Add(meteoro);
        
        SpriteRenderer sr = meteoro.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.8f, 0.2f, 0.2f);
        sr.transform.localScale = Vector3.one * 0.5f;
        
        CircleCollider2D collider = meteoro.AddComponent<CircleCollider2D>();
        collider.radius = 0.3f;
        
        TutorialMissile missileScript = meteoro.AddComponent<TutorialMissile>();
        missileScript.Inicializar(this, velocidadeMeteoros);
        
        meteoro.transform.position = posicao;
    }
    
    Sprite CreateCircleSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dx = x - 16;
                float dy = y - 16;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, dist <= 16 ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    IEnumerator MonitorarOnda()
    {
        while (meteorosAtivos.Count > 0)
        {
            meteorosAtivos.RemoveAll(m => m == null);
            yield return new WaitForSeconds(0.3f);
        }
        
        ondaEmAndamento = false;
    }
    
    public void RegistrarMeteoroDestruido(GameObject meteoro)
    {
        meteorosDestruidos++;
        if (meteorosAtivos.Contains(meteoro))
            meteorosAtivos.Remove(meteoro);
    }
    
    public void RegistrarMeteoroAcertou(GameObject meteoro)
    {
        meteorosAcertaram++;
        if (meteorosAtivos.Contains(meteoro))
            meteorosAtivos.Remove(meteoro);
    }
    
    IEnumerator ResultadoOnda()
    {
        tutorialPanel.SetActive(true);
        
        if (meteorosDestruidos >= meteorosNaOnda - 2)
        {
            tutorialText.text = $"Ótimo! Você destruiu {meteorosDestruidos} de {meteorosNaOnda} meteoros!\n\nContinue assim!";
        }
        else if (meteorosDestruidos >= meteorosNaOnda / 2)
        {
            tutorialText.text = $"Bom trabalho! Você destruiu {meteorosDestruidos} de {meteorosNaOnda} meteoros.\n\nTente acertar mais na próxima!";
        }
        else
        {
            tutorialText.text = $"Você destruiu {meteorosDestruidos} de {meteorosNaOnda} meteoros.\n\nLembre-se: clique nos meteoros para destruí-los!";
        }
        
        continuarButton.onClick.RemoveAllListeners();
        continuarButton.onClick.AddListener(() => {
            tutorialPanel.SetActive(false);
        });
        
        Time.timeScale = 0f;
        
        while (tutorialPanel.activeSelf)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    void FinalizarTutorial()
    {
        Time.timeScale = 1f;
        
        tutorialPanel.SetActive(true);
        tutorialText.text = "Tutorial concluído!\n\nAperte ESC para voltar ao Menu Principal.";
        
        continuarButton.onClick.RemoveAllListeners();
        continuarButton.onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        });
        
        Time.timeScale = 0f;
    }
}
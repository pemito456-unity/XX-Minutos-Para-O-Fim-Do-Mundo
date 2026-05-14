using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class TutorialController2 : MonoBehaviour
{
    [Header("Configurações do Tutorial")]
    [SerializeField] private int meteorosNaOnda = 10;
    [SerializeField] private float tempoEntreMeteoros = 1.5f;
    [SerializeField] private float velocidadeMeteoros = 2f;
    
    [Header("Referências")]
    [SerializeField] private EnemyMissileSpawner_Def spawner;
    [SerializeField] private GameController_Def gameController;
    
    [Header("UI do Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button continuarButton;
    
    [Header("Diálogos do Tutorial (mesma UI)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Botões de Diálogo (3 opções)")]
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Button choiceButton3;
    [SerializeField] private TextMeshProUGUI choiceText1;
    [SerializeField] private TextMeshProUGUI choiceText2;
    [SerializeField] private TextMeshProUGUI choiceText3;
    
    [Header("Diálogos do Tutorial")]
    [SerializeField] private DialogueData dialogoIntroducao;
    [SerializeField] private DialogueData dialogoPressao;
    [SerializeField] private DialogueData dialogoFinal;
    
    [Header("Configurações")]
    [SerializeField] private float reactionTime = 2f;
    
    // Controle do tutorial
    private int meteorosDestruidos = 0;
    private int meteorosAcertaram = 0;
    private bool tutorialEmAndamento = true;
    private bool ondaEmAndamento = false;
    private List<GameObject> meteorosAtivos = new List<GameObject>();
    
    // Controle de diálogo
    private DialogueData currentDialogue;
    private bool isDialogueActive = false;
    private System.Action onDialogueComplete;
    
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
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        
        SetupDialogueButtons();
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Time.timeScale = 0f;
    }
    
    void SetupDialogueButtons()
    {
        if (choiceButton1 != null)
        {
            choiceButton1.onClick.RemoveAllListeners();
            choiceButton1.onClick.AddListener(() => OnDialogueButtonClick(0));
        }
        
        if (choiceButton2 != null)
        {
            choiceButton2.onClick.RemoveAllListeners();
            choiceButton2.onClick.AddListener(() => OnDialogueButtonClick(1));
        }
        
        if (choiceButton3 != null)
        {
            choiceButton3.onClick.RemoveAllListeners();
            choiceButton3.onClick.AddListener(() => OnDialogueButtonClick(2));
        }
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
        
        ShowDialogue(dialogo, () => {
            dialogoCompleto = true;
        });
        
        while (!dialogoCompleto)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    public void ShowDialogue(DialogueData dialogue, System.Action onComplete = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("Diálogo é null!");
            if (onComplete != null) onComplete.Invoke();
            return;
        }
        
        currentDialogue = dialogue;
        onDialogueComplete = onComplete;
        
        StartCoroutine(StartDialogueCoroutine());
    }
    
    IEnumerator StartDialogueCoroutine()
    {
        isDialogueActive = true;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        if (speakerPortraitImage != null && currentDialogue.speakerPortrait != null)
        {
            speakerPortraitImage.sprite = currentDialogue.speakerPortrait;
            speakerPortraitImage.gameObject.SetActive(true);
        }
        
        if (speakerNameText != null)
            speakerNameText.text = currentDialogue.speakerName;
        
        if (dialogueText != null)
            dialogueText.text = currentDialogue.dialogueText;
        
        CreateDialogueButtons(currentDialogue.choices);
        
        Debug.Log($"📖 Tutorial - {currentDialogue.speakerName}: {currentDialogue.dialogueText}");
        
        yield return null;
    }
    
    void CreateDialogueButtons(List<DialogueChoice> choices)
    {
        HideAllChoiceButtons();
        
        int choiceCount = Mathf.Min(choices.Count, 3);
        
        if (choiceCount >= 1 && choiceButton1 != null)
        {
            choiceButton1.gameObject.SetActive(true);
            if (choiceText1 != null) choiceText1.text = choices[0].buttonText;
        }
        
        if (choiceCount >= 2 && choiceButton2 != null)
        {
            choiceButton2.gameObject.SetActive(true);
            if (choiceText2 != null) choiceText2.text = choices[1].buttonText;
        }
        
        if (choiceCount >= 3 && choiceButton3 != null)
        {
            choiceButton3.gameObject.SetActive(true);
            if (choiceText3 != null) choiceText3.text = choices[2].buttonText;
        }
    }
    
    void HideAllChoiceButtons()
    {
        if (choiceButton1 != null) choiceButton1.gameObject.SetActive(false);
        if (choiceButton2 != null) choiceButton2.gameObject.SetActive(false);
        if (choiceButton3 != null) choiceButton3.gameObject.SetActive(false);
    }
    
    public void OnDialogueButtonClick(int buttonIndex)
    {
        if (!isDialogueActive) return;
        
        if (currentDialogue != null && buttonIndex < currentDialogue.choices.Count)
        {
            StartCoroutine(ShowReactionAndClose(currentDialogue.choices[buttonIndex]));
        }
    }
    
    IEnumerator ShowReactionAndClose(DialogueChoice choice)
    {
        HideAllChoiceButtons();
        
        string originalSpeakerName = speakerNameText.text;
        string originalDialogueText = dialogueText.text;
        
        if (!string.IsNullOrEmpty(choice.speakerReaction))
        {
            speakerNameText.text = currentDialogue.speakerName;
            dialogueText.text = choice.speakerReaction;
        }
        else
        {
            speakerNameText.text = "???";
            dialogueText.text = "...";
        }
        
        yield return new WaitForSecondsRealtime(reactionTime);
        
        speakerNameText.text = originalSpeakerName;
        dialogueText.text = originalDialogueText;
        
        CloseDialogue();
    }
    
    void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        isDialogueActive = false;
        
        if (onDialogueComplete != null)
        {
            onDialogueComplete.Invoke();
            onDialogueComplete = null;
        }
        
        currentDialogue = null;
        HideAllChoiceButtons();
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
        meteoro.transform.position = posicao;
        meteoro.tag = "EnemyMissile";
        
        SpriteRenderer sr = meteoro.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.8f, 0.2f, 0.2f);
        sr.transform.localScale = Vector3.one * 0.5f;
        
        CircleCollider2D collider = meteoro.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        
        TutorialMissile missile = meteoro.AddComponent<TutorialMissile>();
        missile.Inicializar(
            () => RegistrarMeteoroDestruido(meteoro),
            () => RegistrarMeteoroAcertou(meteoro),
            velocidadeMeteoros
        );
        
        meteorosAtivos.Add(meteoro);
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
    
    // 🔴 FUNÇÕES PARA REGISTRAR METEOROS
    public void RegistrarMeteoroDestruido(GameObject meteoro)
    {
        meteorosDestruidos++;
        if (meteorosAtivos.Contains(meteoro))
            meteorosAtivos.Remove(meteoro);
        
        Debug.Log($"💥 Meteoro destruído! ({meteorosDestruidos}/{meteorosNaOnda})");
    }
    
    public void RegistrarMeteoroAcertou(GameObject meteoro)
    {
        meteorosAcertaram++;
        if (meteorosAtivos.Contains(meteoro))
            meteorosAtivos.Remove(meteoro);
        
        Debug.Log($"💔 Meteoro acertou o chão! ({meteorosAcertaram}/{meteorosNaOnda})");
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
    
    public void ForceCloseDialogue()
    {
        StopAllCoroutines();
        CloseDialogue();
    }
    
    public bool IsDialogueActive() => isDialogueActive;
}
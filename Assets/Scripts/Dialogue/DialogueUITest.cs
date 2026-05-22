using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class DialogueUITest : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float timeBetweenCalls = 40f;
    [SerializeField] private float ignoreCallPenalty = 15f;
    [SerializeField] private float callRingingDuration = 10f;
    [SerializeField] private float dialogueTimeout = 20f;
    
    [Header("Diálogos")]
    [SerializeField] private List<DialogueData> colonelDialogues;
    [SerializeField] private List<DialogueData> secretaryDialogues;
    [SerializeField] private List<DialogueData> scientistDialogues;
    
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private TMPro.TextMeshProUGUI speakerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    
    [Header("Botões Manuais (arraste os botões aqui)")]
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Button choiceButton3;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText1;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText2;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText3;
    [SerializeField] private GameObject ChoicesContainer;
    
    [Header("Áudio")]
    [SerializeField] private AudioSource phoneAudioSource;
    [SerializeField] private AudioClip phoneRingingClip;
    
    [Header("Efeitos de Tela")]
    [SerializeField] private Canvas targetCanvas;
    
    private GameController_Def gameController;
    private Queue<DialogueData> pendingDialogues = new Queue<DialogueData>();
    private DialogueData currentDialogue;
    private bool isDialogueActive = false;
    private bool isRinging = false;
    private Coroutine ringingCoroutine;
    private Coroutine dialogueTimeoutCoroutine;
    
    private int currentScientistIndex = 0;
    private int randomCallsSinceLastScientist = 0;
    private const int RANDOM_BETWEEN_SCIENTISTS = 2;
    
    private GameObject flashObject;
    private Image flashImage;
    
    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        CreateFlashObject();
        SetupButtons();
        
        StartCoroutine(DialogueScheduler());
        
        Debug.Log($"=== DialogueUITest iniciado ===");
        Debug.Log($"Total de diálogos do cientista: {scientistDialogues.Count}");
    }
    
    void SetupButtons()
    {
        if (choiceButton1 != null)
        {
            choiceButton1.onClick.RemoveAllListeners();
            choiceButton1.onClick.AddListener(() => OnButtonClick(0));
        }
        
        if (choiceButton2 != null)
        {
            choiceButton2.onClick.RemoveAllListeners();
            choiceButton2.onClick.AddListener(() => OnButtonClick(1));
        }
        
        if (choiceButton3 != null)
        {
            choiceButton3.onClick.RemoveAllListeners();
            choiceButton3.onClick.AddListener(() => OnButtonClick(2));
        }
        
        Debug.Log("Botões configurados!");
    }
    
    public void OnButtonClick(int buttonIndex)
    {
        if (dialogueTimeoutCoroutine != null)
        {
            StopCoroutine(dialogueTimeoutCoroutine);
            dialogueTimeoutCoroutine = null;
        }
        
        Debug.Log($" Botão {buttonIndex + 1} clicado! ================");
        
        if (currentDialogue != null && buttonIndex < currentDialogue.choices.Count)
        {
            OnChoiceSelected(currentDialogue.choices[buttonIndex]);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestForceCall();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestForceDialogue();
        }
    }
    
    void HideAllChoiceButtons()
    {
        if (choiceButton1 != null) choiceButton1.gameObject.SetActive(false);
        if (choiceButton2 != null) choiceButton2.gameObject.SetActive(false);
        if (choiceButton3 != null) choiceButton3.gameObject.SetActive(false);
    }
    
    void ShowChoiceButtons(int count)
    {
        HideAllChoiceButtons();
        
        if (count >= 1 && choiceButton1 != null) choiceButton1.gameObject.SetActive(true);
        if (count >= 2 && choiceButton2 != null) choiceButton2.gameObject.SetActive(true);
        if (count >= 3 && choiceButton3 != null) choiceButton3.gameObject.SetActive(true);
    }
    
    void SetChoiceButtonText(int index, string text)
    {
        switch(index)
        {
            case 1: if (choiceText1 != null) choiceText1.text = text; break;
            case 2: if (choiceText2 != null) choiceText2.text = text; break;
            case 3: if (choiceText3 != null) choiceText3.text = text; break;
        }
    }
    
    void CreateDialogueButtons(List<DialogueChoice> choices)
{
    int choiceCount = Mathf.Min(choices.Count, 3);
    ShowChoiceButtons(choiceCount);
    
    Button[] buttons = { choiceButton1, choiceButton2, choiceButton3 };
    TMPro.TextMeshProUGUI[] texts = { choiceText1, choiceText2, choiceText3 };
    
    for (int i = 0; i < choiceCount; i++)
    {
        if (texts[i] != null)
            texts[i].text = choices[i].buttonText;
        
        if (buttons[i] != null)
        {
            ColorBlock colors = buttons[i].colors;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);           // BRANCO normal
            colors.highlightedColor = new Color(0.7f, 0.7f, 0.75f, 1f);  // Cinza mais claro no hover
            colors.pressedColor = new Color(0.4f, 0.4f, 0.45f, 1f);      // Cinza médio ao clicar
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            buttons[i].colors = colors;
        }
    }
    
    Debug.Log($"✅ {choiceCount} botões configurados com hover (brancos)");
}
    
    void CreateFlashObject()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                GameObject canvasGO = new GameObject("TempCanvas");
                targetCanvas = canvasGO.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }
        
        if (targetCanvas == null) return;
        
        flashObject = new GameObject("ScreenFlash");
        flashObject.transform.SetParent(targetCanvas.transform, false);
        
        flashImage = flashObject.AddComponent<Image>();
        flashImage.raycastTarget = false;
        flashImage.color = new Color(0, 0, 0, 0);
        
        RectTransform rect = flashObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();
        
        flashObject.SetActive(false);
    }
    
    void TriggerScreenFlash(Color color, float duration)
    {
        if (flashImage == null) CreateFlashObject();
        if (flashImage != null) StartCoroutine(ScreenFlashCoroutine(color, duration));
    }
    
    IEnumerator ScreenFlashCoroutine(Color color, float duration)
    {
        if (flashObject == null) yield break;
        
        flashObject.SetActive(true);
        flashImage.color = new Color(color.r, color.g, color.b, 0.7f);
        
        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
            float alpha = (timer / duration) * 0.7f;
            flashImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        
        flashObject.SetActive(false);
    }
    
    IEnumerator DialogueScheduler()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenCalls);
            
            if (gameController != null && 
                gameController.currentState == GameController_Def.GameState.Gameplay && 
                !isDialogueActive && 
                !isRinging &&
                pendingDialogues.Count == 0)
            {
                Debug.Log($"Scheduler: Nova chamada após {timeBetweenCalls}s");
                ScheduleNextDialogue();
            }
        }
    }
    
    void ScheduleNextDialogue()
    {
        DialogueData nextDialogue = GetNextDialogue();
        
        if (nextDialogue != null)
        {
            pendingDialogues.Enqueue(nextDialogue);
            Debug.Log($"Novo diálogo na fila: {nextDialogue.speakerName} (Tipo: {nextDialogue.speakerType})");
            StartRinging();
        }
        else
        {
            Debug.LogError("GetNextDialogue retornou NULL!");
        }
    }
    
    DialogueData GetNextDialogue()
    {
        if (currentScientistIndex < scientistDialogues.Count)
        {
            if (randomCallsSinceLastScientist >= RANDOM_BETWEEN_SCIENTISTS)
            {
                DialogueData scientist = scientistDialogues[currentScientistIndex];
                currentScientistIndex++;
                randomCallsSinceLastScientist = 0;
                Debug.Log($"🔬 CIENTISTA {currentScientistIndex}/{scientistDialogues.Count}");
                return scientist;
            }
            else
            {
                DialogueData random = GetRandomNonScientistDialogue();
                if (random != null)
                {
                    randomCallsSinceLastScientist++;
                    Debug.Log($"ALEATÓRIO {randomCallsSinceLastScientist}/{RANDOM_BETWEEN_SCIENTISTS}");
                    return random;
                }
            }
        }
        
        if (currentScientistIndex >= scientistDialogues.Count && scientistDialogues.Count > 0)
        {
            Debug.Log("VITÓRIA!");
            if (gameController != null)
                gameController.TriggerVictory();
            return null;
        }
        
        return GetRandomNonScientistDialogue();
    }
    
    DialogueData GetRandomNonScientistDialogue()
    {
        List<DialogueData> availableDialogues = new List<DialogueData>();
        availableDialogues.AddRange(colonelDialogues);
        availableDialogues.AddRange(secretaryDialogues);
        
        if (availableDialogues.Count == 0) return null;
        
        return availableDialogues[Random.Range(0, availableDialogues.Count)];
    }
    
    void StartRinging()
    {
        if (pendingDialogues.Count == 0) return;
        
        if (isRinging)
        {
            Debug.LogWarning("Já está tocando!");
            return;
        }
        
        Debug.Log("TELEFONE TOCANDO!");
        isRinging = true;
        
        if (phoneAudioSource != null && phoneRingingClip != null)
        {
            phoneAudioSource.clip = phoneRingingClip;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
            Debug.Log("🔊 Áudio do telefone começou a tocar");
        }
        
        Debug.Log(" Abrindo diálogo...");
        
        if (ringingCoroutine != null) StopCoroutine(ringingCoroutine);
        ringingCoroutine = StartCoroutine(RingingTimerCoroutine());
        
        AnswerPhone();
    }
    
    IEnumerator RingingTimerCoroutine()
    {
        float elapsed = 0f;
        
        while (elapsed < callRingingDuration && isRinging)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (isRinging && pendingDialogues.Count > 0)
        {
            DialogueData ignoredDialogue = pendingDialogues.Dequeue();
            Debug.Log($"Chamada ignorada! Penalidade: +{ignoreCallPenalty} pressão");
            
            if (gameController != null)
                gameController.ModifyPressureByDialogue(ignoreCallPenalty);
            
            if (phoneAudioSource != null && phoneAudioSource.isPlaying)
            {
                phoneAudioSource.Stop();
                phoneAudioSource.loop = false;
            }
            
            StopRinging();
        }
    }
    
    void StopRinging()
    {
        Debug.Log("Telefone parou de tocar (StopRinging)");
        isRinging = false;
        
        if (ringingCoroutine != null)
        {
            StopCoroutine(ringingCoroutine);
            ringingCoroutine = null;
        }
    }
    
    public void AnswerPhone()
    {
        Debug.Log($"Atendendo chamada! pending={pendingDialogues.Count}");
        
        if (!isDialogueActive && pendingDialogues.Count > 0)
        {
            Debug.Log("Iniciando diálogo!");
            
            isRinging = false;
            
            if (ringingCoroutine != null)
            {
                StopCoroutine(ringingCoroutine);
                ringingCoroutine = null;
            }
            
            currentDialogue = pendingDialogues.Dequeue();
            StartDialogue();
        }
    }
    
    void StartDialogue()
    {
        if (currentDialogue == null)
        {
            Debug.LogError("currentDialogue é null!");
            return;
        }
        
        Debug.Log($"Iniciando diálogo: {currentDialogue.speakerName}");
        
        isDialogueActive = true;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        if (speakerPortraitImage != null)
        {
            if (currentDialogue.speakerPortrait != null)
            {
                speakerPortraitImage.sprite = currentDialogue.speakerPortrait;
                speakerPortraitImage.gameObject.SetActive(true);
            }
            else
            {
                speakerPortraitImage.gameObject.SetActive(false);
            }
        }
        
        if (speakerNameText != null)
            speakerNameText.text = currentDialogue.speakerName;
        
        if (dialogueText != null)
            dialogueText.text = currentDialogue.dialogueText;
        
        CreateDialogueButtons(currentDialogue.choices);
        
        if (dialogueTimeoutCoroutine != null)
            StopCoroutine(dialogueTimeoutCoroutine);
        dialogueTimeoutCoroutine = StartCoroutine(DialogueTimeoutCoroutine());
        
        Debug.Log($"Diálogo exibido! ({currentDialogue.choices.Count} opções) - Áudio do telefone continua tocando");
    }
    
    IEnumerator DialogueTimeoutCoroutine()
    {
        float elapsed = 0f;
        
        while (elapsed < dialogueTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (isDialogueActive && currentDialogue != null)
        {
            Debug.Log($"⏰ Diálogo ignorado! Nenhuma escolha foi feita em {dialogueTimeout} segundos. Penalidade: +{ignoreCallPenalty} pressão");
            
            if (gameController != null)
                gameController.ModifyPressureByDialogue(ignoreCallPenalty);
            
            EndDialogue();
        }
    }
    
    void OnChoiceSelected(DialogueChoice choice)
    {
        if (dialogueTimeoutCoroutine != null)
        {
            StopCoroutine(dialogueTimeoutCoroutine);
            dialogueTimeoutCoroutine = null;
        }
        
        Debug.Log($"Escolha: {choice.buttonText} | Pressão: {(choice.pressureChange >= 0 ? "+" : "")}{choice.pressureChange}");
        
        if (gameController != null)
        {
            gameController.ModifyPressureByDialogue(choice.pressureChange);
            if (choice.advancesInvestigation)
                gameController.AdvanceInvestigation();
        }
        
        if (choice.screenFlashColor != null && choice.screenFlashColor.a > 0)
            TriggerScreenFlash(choice.screenFlashColor, choice.screenFlashDuration);
        
        StartCoroutine(ShowReactionAndClose(choice));
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
            Debug.Log($"🎭 Reação: {currentDialogue.speakerName}: {choice.speakerReaction}");
        }
        else
        {
            speakerNameText.text = "???";
            dialogueText.text = "...";
        }
        
        yield return new WaitForSecondsRealtime(2f);
        
        speakerNameText.text = originalSpeakerName;
        dialogueText.text = originalDialogueText;
        
        EndDialogue();
    }
    
    void EndDialogue()
    {
        Debug.Log("Fechando diálogo");
        
        isDialogueActive = false;
        
        if (dialogueTimeoutCoroutine != null)
        {
            StopCoroutine(dialogueTimeoutCoroutine);
            dialogueTimeoutCoroutine = null;
        }
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        currentDialogue = null;
        
        HideAllChoiceButtons();
        
        if (phoneAudioSource != null && phoneAudioSource.isPlaying)
        {
            phoneAudioSource.Stop();
            phoneAudioSource.loop = false;
            Debug.Log("🔊 Áudio do telefone parou (diálogo fechado)");
        }
    }
    
    public void TestForceCall()
    {
        Debug.Log("=== TESTE: Forçando chamada ===");
        
        if (!isRinging && !isDialogueActive)
        {
            ScheduleNextDialogue();
        }
    }
    
    public void TestForceDialogue()
    {
        Debug.Log("=== TESTE: Forçando diálogo direto ===");
        
        if (colonelDialogues.Count > 0)
        {
            currentDialogue = colonelDialogues[0];
            StartDialogue();
        }
        else if (scientistDialogues.Count > 0)
        {
            currentDialogue = scientistDialogues[0];
            StartDialogue();
        }
        else
        {
            Debug.LogError("Nenhum diálogo disponível!");
        }
    }
    
    public void ResetDialogueSequence()
    {
        currentScientistIndex = 0;
        randomCallsSinceLastScientist = 0;
        pendingDialogues.Clear();
        
        if (phoneAudioSource != null && phoneAudioSource.isPlaying)
        {
            phoneAudioSource.Stop();
            phoneAudioSource.loop = false;
        }
        
        StopRinging();
        Debug.Log("Sequência de diálogos resetada!");
    }
    
    public bool IsRinging() => isRinging;
}
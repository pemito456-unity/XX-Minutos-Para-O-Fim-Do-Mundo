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
    [Tooltip("Quantas chamadas normais antes de um diálogo adiado com E voltar a tocar.")]
    [SerializeField] private int normalCallsBeforePostponed = 1;
    
    [Header("Diálogos")]
    [SerializeField] private List<DialogueData> colonelDialogues;
    [SerializeField] private List<DialogueData> secretaryDialogues;
    [SerializeField] private List<ScientistConversationData> scientistConversations;
    
    [Header("Primeiro Diálogo (Obrigatório)")]
    [SerializeField] private DialogueData primeiroDialogo;
    
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private TMPro.TextMeshProUGUI speakerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    
    [Header("Botões Manuais")]
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

    [Header("Feedback Sonoro das Respostas")]
    [SerializeField] private AudioSource responseFeedbackAudioSource;
    [SerializeField] private AudioClip positiveResponseClip;
    [SerializeField] private AudioClip negativeResponseClip;
    [SerializeField] [Range(0f, 1f)] private float responseFeedbackVolume = 1f;
    
    [Header("Efeitos de Tela")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Telefone")]
    [SerializeField] private GameObject phoneButtonObject;
    [SerializeField] private GameObject phoneButtonPrefab;
    
    private struct PendingPhoneCall
    {
        public DialogueData Standard;
        public ScientistConversationData Scientist;
        public bool IsScientist => Scientist != null;
    }

    private GameController_Def gameController;
    private List<DialogueChoice> shuffledChoices = new List<DialogueChoice>();
    private bool hasIncomingCall;
    private PendingPhoneCall incomingCall;
    private bool hasActiveDialogueCall;
    private PendingPhoneCall activeDialogueCall;
    private readonly Queue<PendingPhoneCall> postponedCalls = new Queue<PendingPhoneCall>();
    private int normalCallsSincePostpone;
    private DialogueData currentDialogue;
    private ScientistConversationData currentScientistConversation;
    private ConversationPlayerChoice currentScientistChoice;
    private bool isDialogueActive = false;
    private bool isScientistConversation = false;
    private bool waitingForScientistChoice = false;
    private bool isRinging = false;
    private Coroutine ringingCoroutine;
    private Coroutine dialogueTimeoutCoroutine;
    private Coroutine scientistConversationCoroutine;
    
    private int currentScientistIndex = 0;
    private int randomCallsSinceLastScientist = 0;
    private const int RANDOM_BETWEEN_SCIENTISTS = 2;
    
    private bool primeiroDialogoRealizado = false;
    
    private GameObject flashObject;
    private Image flashImage;
    
    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        GarantirAudioSourceResposta();

        GarantirBotaoTelefone();
        ApplyPhoneButtonHover();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (phoneButtonObject != null)
            phoneButtonObject.SetActive(false);

        CreateFlashObject();
        SetupButtons();
        
        StartCoroutine(DialogueScheduler());
        
        Debug.Log($"=== DialogueUITest iniciado ===");
        Debug.Log($"Total de conversas do cientista: {scientistConversations.Count}");
        
        if (primeiroDialogo != null)
        {
            Debug.Log($"Primeiro diálogo configurado: {primeiroDialogo.speakerName}");
        }
        else
        {
            Debug.LogWarning("Primeiro diálogo não configurado! O jogo começará com chamadas normais.");
        }
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

        if (isScientistConversation)
        {
            if (buttonIndex == 0 && waitingForScientistChoice && currentScientistChoice != null)
                OnScientistChoiceSelected(currentScientistChoice);
            return;
        }
        
        Debug.Log($" Botão {buttonIndex + 1} clicado! ================");
        
        if (buttonIndex >= 0 && buttonIndex < shuffledChoices.Count)
            OnChoiceSelected(shuffledChoices[buttonIndex]);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            TestForceCall();

        if (Input.GetKeyDown(KeyCode.R))
            TestForceDialogue();

        if (Input.GetKeyDown(KeyCode.E) && isDialogueActive)
            SkipActiveDialogue();
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
        shuffledChoices = EmbaralharLista(choices);
        int choiceCount = Mathf.Min(shuffledChoices.Count, 3);
        ShowChoiceButtons(choiceCount);

        Button[] buttons = { choiceButton1, choiceButton2, choiceButton3 };
        TMPro.TextMeshProUGUI[] texts = { choiceText1, choiceText2, choiceText3 };

        for (int i = 0; i < choiceCount; i++)
        {
            if (texts[i] != null)
                texts[i].text = shuffledChoices[i].buttonText;

            DialogueButtonStyling.ApplyChoiceButtonHover(buttons[i]);
        }

        Debug.Log($"✅ {choiceCount} botões configurados (ordem aleatória)");
    }

    void ShowSingleChoiceButton(ConversationPlayerChoice choice)
    {
        HideAllChoiceButtons();

        if (choice == null || choiceButton1 == null)
            return;

        choiceButton1.gameObject.SetActive(true);
        if (choiceText1 != null)
            choiceText1.text = choice.buttonText;

        DialogueButtonStyling.ApplyChoiceButtonHover(choiceButton1);
    }

    static List<DialogueChoice> EmbaralharLista(List<DialogueChoice> original)
    {
        List<DialogueChoice> lista = new List<DialogueChoice>(original);
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
        return lista;
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
                !hasIncomingCall)
            {
                Debug.Log($"Scheduler: Nova chamada após {timeBetweenCalls}s");
                ScheduleNextDialogue();
            }
        }
    }
    
    void ScheduleNextDialogue()
    {
        if (hasIncomingCall || isRinging)
        {
            Debug.LogWarning("Já existe uma chamada ativa; nova chamada não foi agendada.");
            return;
        }

        PendingPhoneCall nextCall = GetNextScheduledPhoneCall();
        
        if (nextCall.IsScientist || nextCall.Standard != null)
        {
            incomingCall = nextCall;
            hasIncomingCall = true;
            string nome = nextCall.IsScientist
                ? nextCall.Scientist.speakerName
                : nextCall.Standard.speakerName;
            Debug.Log($"Nova chamada recebida: {nome}");
            StartRinging();
        }
        else
        {
            Debug.LogError("GetNextPhoneCall retornou chamada vazia!");
        }
    }
    
    PendingPhoneCall GetNextScheduledPhoneCall()
    {
        if (!primeiroDialogoRealizado && primeiroDialogo != null)
        {
            primeiroDialogoRealizado = true;
            Debug.Log($"🔴 PRIMEIRO DIÁLOGO: {primeiroDialogo.speakerName}");
            return new PendingPhoneCall { Standard = primeiroDialogo };
        }
        
        if (postponedCalls.Count > 0 && normalCallsSincePostpone >= Mathf.Max(1, normalCallsBeforePostponed))
        {
            normalCallsSincePostpone = 0;
            PendingPhoneCall postponed = postponedCalls.Dequeue();
            Debug.Log($"Chamada adiada (E) reordenada — voltando: {GetCallDisplayName(postponed)}");
            return postponed;
        }

        normalCallsSincePostpone++;
        return PickNextPhoneCall();
    }

    PendingPhoneCall PickNextPhoneCall()
    {
        if (currentScientistIndex < scientistConversations.Count)
        {
            if (randomCallsSinceLastScientist >= RANDOM_BETWEEN_SCIENTISTS)
            {
                ScientistConversationData scientist = scientistConversations[currentScientistIndex];
                randomCallsSinceLastScientist = 0;
                Debug.Log($"🔬 Chamada do cientista ({currentScientistIndex + 1}/{scientistConversations.Count})");
                return new PendingPhoneCall { Scientist = scientist };
            }

            DialogueData random = GetRandomNonScientistDialogue();
            if (random != null)
            {
                randomCallsSinceLastScientist++;
                Debug.Log($"ALEATÓRIO {randomCallsSinceLastScientist}/{RANDOM_BETWEEN_SCIENTISTS}");
                return new PendingPhoneCall { Standard = random };
            }
        }
        
        if (currentScientistIndex >= scientistConversations.Count && scientistConversations.Count > 0)
        {
            Debug.Log("VITÓRIA!");
            if (gameController != null)
                gameController.TriggerVictory();
            return default;
        }
        
        DialogueData fallback = GetRandomNonScientistDialogue();
        return fallback != null
            ? new PendingPhoneCall { Standard = fallback }
            : default;
    }
    
    // 🔴 CORRIGIDO: agora escolhe um diálogo aleatório da lista
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
        if (!hasIncomingCall)
            return;
        
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
        
        if (phoneButtonObject != null)
            phoneButtonObject.SetActive(true);

        if (ringingCoroutine != null)
            StopCoroutine(ringingCoroutine);

        ringingCoroutine = StartCoroutine(RingingTimerCoroutine());
    }
    
    IEnumerator RingingTimerCoroutine()
    {
        float elapsed = 0f;
        
        while (elapsed < callRingingDuration)
        {
            if (!isRinging)
            {
                ringingCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (isRinging && hasIncomingCall)
            HandleCallIgnored();

        ringingCoroutine = null;
    }

    void HandleCallIgnored()
    {
        if (!hasIncomingCall)
            return;

        string callerName = incomingCall.IsScientist
            ? incomingCall.Scientist.speakerName
            : incomingCall.Standard.speakerName;

        Debug.Log($"Chamada ignorada ({callerName})! Penalidade: +{ignoreCallPenalty} pressão política");

        ClearIncomingCall();
        ApplyIgnoredCallPenalty();
        EndRingingState(stopTimerCoroutine: false);
    }

    void ApplyIgnoredCallPenalty()
    {
        if (gameController == null)
            gameController = Object.FindAnyObjectByType<GameController_Def>();

        if (gameController == null)
        {
            Debug.LogWarning("GameController não encontrado — penalidade de chamada ignorada não aplicada.");
            return;
        }

        gameController.AddYellowPressure(ignoreCallPenalty);
        TriggerScreenFlash(new Color(1f, 0.12f, 0.08f, 1f), 0.35f);
        PlayNegativeResponseFeedbackSound();
    }

    void ClearIncomingCall()
    {
        hasIncomingCall = false;
        incomingCall = default;
    }

    void EndRingingState(bool stopTimerCoroutine)
    {
        isRinging = false;

        if (phoneButtonObject != null)
            phoneButtonObject.SetActive(false);

        StopPhoneRingingAudio();

        if (stopTimerCoroutine && ringingCoroutine != null)
        {
            StopCoroutine(ringingCoroutine);
            ringingCoroutine = null;
        }
    }

    void StopPhoneRingingAudio()
    {
        if (phoneAudioSource != null && phoneAudioSource.isPlaying)
        {
            phoneAudioSource.Stop();
            phoneAudioSource.loop = false;
        }
    }
    
    void StopRinging()
    {
        EndRingingState(stopTimerCoroutine: true);
    }
    
    public void AnswerPhone()
    {
        if (isDialogueActive || !hasIncomingCall)
            return;

        PendingPhoneCall call = incomingCall;
        ClearIncomingCall();
        EndRingingState(stopTimerCoroutine: true);
        BeginPhoneCall(call);
    }

    void BeginPhoneCall(PendingPhoneCall call)
    {
        activeDialogueCall = call;
        hasActiveDialogueCall = true;

        if (call.IsScientist)
            StartScientistConversation(call.Scientist);
        else
        {
            currentDialogue = call.Standard;
            StartDialogue();
        }
    }

    void SkipActiveDialogue()
    {
        if (!isDialogueActive || !hasActiveDialogueCall)
            return;

        PendingPhoneCall skipped = activeDialogueCall;
        postponedCalls.Enqueue(skipped);
        hasActiveDialogueCall = false;
        activeDialogueCall = default;
        normalCallsSincePostpone = 0;

        Debug.Log($"Diálogo pulado (E). Reordenado para depois: {GetCallDisplayName(skipped)}");

        EndDialogue();

        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;
    }

    static string GetCallDisplayName(PendingPhoneCall call)
    {
        if (call.IsScientist)
            return call.Scientist != null ? call.Scientist.speakerName : "Cientista";

        return call.Standard != null ? call.Standard.speakerName : "Desconhecido";
    }

    void CompleteScientistConversation()
    {
        currentScientistIndex++;
        randomCallsSinceLastScientist = 0;
        Debug.Log($"Conversa do cientista concluída. Progresso: {currentScientistIndex}/{scientistConversations.Count}");
    }
    
    void StartDialogue()
    {
        if (currentDialogue == null)
        {
            Debug.LogError("currentDialogue é null!");
            return;
        }
        
        Debug.Log($"Iniciando diálogo: {currentDialogue.speakerName}");
        
        isScientistConversation = false;
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

    void StartScientistConversation(ScientistConversationData conversation)
    {
        if (conversation == null || conversation.exchanges == null || conversation.exchanges.Count == 0)
        {
            Debug.LogError("Conversa do cientista inválida ou sem trocas de fala!");
            return;
        }

        currentScientistConversation = conversation;
        currentDialogue = null;
        isScientistConversation = true;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerPortraitImage != null)
        {
            if (conversation.speakerPortrait != null)
            {
                speakerPortraitImage.sprite = conversation.speakerPortrait;
                speakerPortraitImage.gameObject.SetActive(true);
            }
            else
            {
                speakerPortraitImage.gameObject.SetActive(false);
            }
        }

        if (scientistConversationCoroutine != null)
            StopCoroutine(scientistConversationCoroutine);

        scientistConversationCoroutine = StartCoroutine(RunScientistConversationCoroutine());
        Debug.Log($"Conversa do cientista iniciada ({conversation.exchanges.Count} trocas)");
    }

    IEnumerator RunScientistConversationCoroutine()
    {
        List<ConversationExchange> exchanges = currentScientistConversation.exchanges;

        for (int i = 0; i < exchanges.Count; i++)
        {
            ConversationExchange exchange = exchanges[i];
            if (exchange == null)
                continue;

            if (speakerNameText != null)
                speakerNameText.text = currentScientistConversation.speakerName;

            if (dialogueText != null)
                dialogueText.text = exchange.scientistLine;

            HideAllChoiceButtons();
            yield return new WaitForSecondsRealtime(0.35f);

            ConversationPlayerChoice playerChoice = exchange.playerResponse;
            if (playerChoice == null || string.IsNullOrWhiteSpace(playerChoice.buttonText))
            {
                Debug.LogWarning($"Troca {i + 1} sem resposta do jogador; pulando botão.");
                continue;
            }

            waitingForScientistChoice = true;
            currentScientistChoice = playerChoice;
            ShowSingleChoiceButton(playerChoice);
            StartScientistResponseTimeout();

            while (waitingForScientistChoice)
                yield return null;
        }

        if (!string.IsNullOrWhiteSpace(currentScientistConversation.closingScientistLine))
        {
            if (speakerNameText != null)
                speakerNameText.text = currentScientistConversation.speakerName;

            if (dialogueText != null)
                dialogueText.text = currentScientistConversation.closingScientistLine;

            HideAllChoiceButtons();
            yield return new WaitForSecondsRealtime(2f);
        }

        if (currentScientistConversation.advancesInvestigationOnComplete && gameController != null)
            gameController.AdvanceInvestigation();

        CompleteScientistConversation();
        EndDialogue();
    }

    void StartScientistResponseTimeout()
    {
        if (dialogueTimeoutCoroutine != null)
            StopCoroutine(dialogueTimeoutCoroutine);

        float timeout = currentScientistConversation != null
            ? currentScientistConversation.timeToDecidePerResponse
            : dialogueTimeout;

        dialogueTimeoutCoroutine = StartCoroutine(ScientistResponseTimeoutCoroutine(timeout));
    }

    IEnumerator ScientistResponseTimeoutCoroutine(float timeout)
    {
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!isScientistConversation || !waitingForScientistChoice)
            yield break;

        Debug.Log($"⏰ Sem resposta em {timeout}s na conversa do cientista. Penalidade: +{ignoreCallPenalty} pressão");

        if (gameController != null)
            gameController.ModifyPressureByDialogue(ignoreCallPenalty);

        waitingForScientistChoice = false;
        EndDialogue();
    }

    void OnScientistChoiceSelected(ConversationPlayerChoice choice)
    {
        waitingForScientistChoice = false;
        currentScientistChoice = null;

        if (dialogueTimeoutCoroutine != null)
        {
            StopCoroutine(dialogueTimeoutCoroutine);
            dialogueTimeoutCoroutine = null;
        }

        Debug.Log($"Resposta (cientista): {choice.buttonText} | Pressão: {(choice.pressureChange >= 0 ? "+" : "")}{choice.pressureChange}");

        if (gameController != null)
        {
            gameController.ModifyPressureByDialogue(choice.pressureChange);
            if (choice.advancesInvestigation)
                gameController.AdvanceInvestigation();
        }

        PlayChoicePressureFlash(choice.pressureChange);
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
        
        PlayChoicePressureFlash(choice.pressureChange);

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
        
        hasActiveDialogueCall = false;
        activeDialogueCall = default;
        isDialogueActive = false;
        isScientistConversation = false;
        waitingForScientistChoice = false;
        currentScientistChoice = null;

        if (scientistConversationCoroutine != null)
        {
            StopCoroutine(scientistConversationCoroutine);
            scientistConversationCoroutine = null;
        }
        
        if (dialogueTimeoutCoroutine != null)
        {
            StopCoroutine(dialogueTimeoutCoroutine);
            dialogueTimeoutCoroutine = null;
        }
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        currentDialogue = null;
        currentScientistConversation = null;
        
        HideAllChoiceButtons();
        shuffledChoices.Clear();

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
        
        if (!isRinging && !isDialogueActive && !hasIncomingCall)
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
        else if (scientistConversations.Count > 0)
        {
            StartScientistConversation(scientistConversations[0]);
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
        normalCallsSincePostpone = 0;
        postponedCalls.Clear();
        hasActiveDialogueCall = false;
        activeDialogueCall = default;
        ClearIncomingCall();
        
        if (phoneAudioSource != null && phoneAudioSource.isPlaying)
        {
            phoneAudioSource.Stop();
            phoneAudioSource.loop = false;
        }
        
        StopRinging();
        Debug.Log("Sequência de diálogos resetada!");
    }
    
    public bool IsRinging() => isRinging;

    void GarantirBotaoTelefone()
    {
        if (phoneButtonObject != null)
            return;

        PhoneButton existente = Object.FindAnyObjectByType<PhoneButton>();
        if (existente != null)
        {
            phoneButtonObject = existente.gameObject;
            return;
        }

        if (phoneButtonPrefab == null)
            return;

        Transform parent = null;
        if (dialoguePanel != null)
            parent = dialoguePanel.transform.parent;

        if (parent == null && targetCanvas != null)
            parent = targetCanvas.transform;

        if (parent == null)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
                parent = canvas.transform;
        }

        if (parent == null)
            return;

        phoneButtonObject = Instantiate(phoneButtonPrefab, parent);
        phoneButtonObject.SetActive(false);
        ApplyPhoneButtonHover();
    }

    void ApplyPhoneButtonHover()
    {
        if (phoneButtonObject == null)
            return;

        Button phoneButton = phoneButtonObject.GetComponent<Button>();
        DialogueButtonStyling.ApplyChoiceButtonHover(phoneButton);
    }

    void PlayChoicePressureFlash(float pressureChange)
    {
        if (Mathf.Approximately(pressureChange, 0f))
            return;

        if (pressureChange > 0f)
        {
            TriggerScreenFlash(new Color(1f, 0.12f, 0.08f, 1f), 0.35f);
            PlayNegativeResponseFeedbackSound();
        }
        else
        {
            TriggerScreenFlash(new Color(1f, 0.92f, 0.12f, 1f), 0.35f);
            PlayPositiveResponseFeedbackSound();
        }
    }

    void GarantirAudioSourceResposta()
    {
        if (responseFeedbackAudioSource != null)
            return;

        foreach (AudioSource source in GetComponents<AudioSource>())
        {
            if (source != phoneAudioSource)
            {
                responseFeedbackAudioSource = source;
                break;
            }
        }

        if (responseFeedbackAudioSource == null)
            responseFeedbackAudioSource = gameObject.AddComponent<AudioSource>();

        responseFeedbackAudioSource.playOnAwake = false;
        responseFeedbackAudioSource.loop = false;
    }

    void PlayPositiveResponseFeedbackSound()
    {
        if (positiveResponseClip == null)
            return;

        GarantirAudioSourceResposta();
        if (responseFeedbackAudioSource != null)
            responseFeedbackAudioSource.PlayOneShot(positiveResponseClip, responseFeedbackVolume);
    }

    void PlayNegativeResponseFeedbackSound()
    {
        if (negativeResponseClip == null)
            return;

        GarantirAudioSourceResposta();
        if (responseFeedbackAudioSource != null)
            responseFeedbackAudioSource.PlayOneShot(negativeResponseClip, responseFeedbackVolume);
    }
}
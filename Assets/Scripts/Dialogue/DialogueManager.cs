using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float timeBetweenCalls = 40f;
    [SerializeField] private float ignoreCallPenalty = 15f;
    [SerializeField] private float callRingingDuration = 10f;
    
    [Header("Diálogos")]
    [SerializeField] private List<DialogueData> colonelDialogues;
    [SerializeField] private List<DialogueData> secretaryDialogues;
    [SerializeField] private List<DialogueData> scientistDialogues;
    
    [Header("UI Existente")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private TMPro.TextMeshProUGUI speakerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    
    [Header("Telefone")]
    [SerializeField] private GameObject phoneButtonObject;
    [SerializeField] private AudioSource phoneAudioSource;
    [SerializeField] private AudioClip phoneRingingClip;
    
    [Header("Reação do Interlocutor")]
    [SerializeField] private GameObject reactionPanel;
    [SerializeField] private TMPro.TextMeshProUGUI reactionText;
    [SerializeField] private float reactionDisplayTime = 2f;
    
    [Header("Efeitos de Tela")]
    [SerializeField] private Canvas targetCanvas;
    
    private GameController_Def gameController;
    private Queue<DialogueData> pendingDialogues = new Queue<DialogueData>();
    private DialogueData currentDialogue;
    private bool isDialogueActive = false;
    private bool isRinging = false;
    private float ringingTimer;
    private Coroutine ringingCoroutine;
    private Coroutine reactionCoroutine;
    
    private int scientistCallCount = 0;
    private int nonScientistCallsSinceLastScientist = 0;
    private const int CALLS_BEFORE_SCIENTIST = 2;
    
    private float decisionTimer;
    
    private GameObject flashObject;
    private Image flashImage;
    
    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        
        // Garante que a UI começa desativada
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (phoneButtonObject != null)
            phoneButtonObject.SetActive(false);
        
        if (reactionPanel != null)
            reactionPanel.SetActive(false);
        
        CreateFlashObject();
        StartCoroutine(DialogueScheduler());
        
        Debug.Log($"DialogueManager iniciado. Diálogos - Coronel: {colonelDialogues.Count}, Secretário: {secretaryDialogues.Count}, Cientista: {scientistDialogues.Count}");
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
                !isRinging)
            {
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
            Debug.Log($"Novo diálogo na fila: {nextDialogue.speakerName}");
            StartRinging();
        }
    }
    
    DialogueData GetNextDialogue()
    {
        if (scientistCallCount < scientistDialogues.Count)
        {
            if (nonScientistCallsSinceLastScientist >= CALLS_BEFORE_SCIENTIST)
            {
                var scientistDialogue = scientistDialogues[scientistCallCount];
                scientistCallCount++;
                nonScientistCallsSinceLastScientist = 0;
                Debug.Log($"Sequência: CIENTISTA {scientistCallCount}/{scientistDialogues.Count}");
                return scientistDialogue;
            }
            else
            {
                DialogueData randomDialogue = GetRandomNonScientistDialogue();
                if (randomDialogue != null)
                {
                    nonScientistCallsSinceLastScientist++;
                    return randomDialogue;
                }
            }
        }
        
        if (scientistCallCount >= scientistDialogues.Count && scientistDialogues.Count > 0)
        {
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
        
        isRinging = true;
        ringingTimer = callRingingDuration;
        
        if (phoneButtonObject != null)
            phoneButtonObject.SetActive(true);
        
        if (phoneAudioSource != null && phoneRingingClip != null)
        {
            phoneAudioSource.clip = phoneRingingClip;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }
        
        if (ringingCoroutine != null) StopCoroutine(ringingCoroutine);
        ringingCoroutine = StartCoroutine(RingingTimer());
        
        Debug.Log("Telefone tocando...");
    }
    
    IEnumerator RingingTimer()
    {
        while (ringingTimer > 0)
        {
            ringingTimer -= Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (isRinging && pendingDialogues.Count > 0)
        {
            DialogueData ignoredDialogue = pendingDialogues.Dequeue();
            Debug.Log($"Chamada ignorada! Penalidade de {ignoreCallPenalty}");
            
            if (gameController != null)
                gameController.ModifyPressureByDialogue(ignoreCallPenalty);
            
            StopRinging();
            
            if (ignoredDialogue.speakerType == SpeakerType.Scientist)
            {
                pendingDialogues.Enqueue(ignoredDialogue);
            }
            else if (nonScientistCallsSinceLastScientist > 0)
            {
                nonScientistCallsSinceLastScientist--;
            }
        }
    }
    
    void StopRinging()
    {
        isRinging = false;
        
        if (phoneButtonObject != null)
            phoneButtonObject.SetActive(false);
        
        if (phoneAudioSource != null)
        {
            phoneAudioSource.Stop();
            phoneAudioSource.loop = false;
        }
        
        if (ringingCoroutine != null) StopCoroutine(ringingCoroutine);
    }
    
    public void AnswerPhone()
    {
        if (!isDialogueActive && isRinging && pendingDialogues.Count > 0)
        {
            StopRinging();
            currentDialogue = pendingDialogues.Dequeue();
            StartDialogue();
        }
    }
    
    void StartDialogue()
    {
        if (currentDialogue == null) return;
        
        Debug.Log($"Iniciando diálogo: {currentDialogue.speakerName}");
        
        isDialogueActive = true;
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Dialogue;
        
        Time.timeScale = 0f;
        
        // Ativa o painel
        dialoguePanel.SetActive(true);
        
        // Configura a imagem (se existir)
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
        
        // Configura textos
        if (speakerNameText != null)
            speakerNameText.text = currentDialogue.speakerName;
        
        if (dialogueText != null)
            dialogueText.text = currentDialogue.dialogueText;
        
        decisionTimer = currentDialogue.timeToDecide;
        
        // Toca áudio de início (se houver)
        if (currentDialogue.dialogueStartClip != null && phoneAudioSource != null)
            phoneAudioSource.PlayOneShot(currentDialogue.dialogueStartClip);
        
        // LIMPA e CRIA os botões
        if (choicesContainer != null)
        {
            // Remove todos os botões antigos
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Cria novos botões
            foreach (var choice in currentDialogue.choices)
            {
                if (choiceButtonPrefab != null)
                {
                    GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
                    
                    // Configura o texto do botão
                    TMPro.TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = choice.buttonText;
                    
                    // Configura o clique
                    Button button = buttonObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => OnChoiceSelected(choice));
                    }
                }
                else
                {
                    Debug.LogError("ChoiceButtonPrefab não está atribuído!");
                }
            }
        }
        
        // Força o layout a se atualizar
        Canvas.ForceUpdateCanvases();
        
        if (currentDialogue.isMandatory)
            decisionTimer = 999f;
    }
    
    void OnChoiceSelected(DialogueChoice choice)
    {
        // Aplica consequências
        if (gameController != null)
        {
            gameController.ModifyPressureByDialogue(choice.pressureChange);
            if (choice.advancesInvestigation)
                gameController.AdvanceInvestigation();
        }
        
        // Mostra reação
        if (!string.IsNullOrEmpty(choice.speakerReaction))
            ShowReaction(choice.speakerReaction);
        
        // Toca som da reação
        if (choice.reactionSound != null && phoneAudioSource != null)
            phoneAudioSource.PlayOneShot(choice.reactionSound);
        
        // Flash na tela
        if (choice.screenFlashColor != null && choice.screenFlashColor.a > 0)
            TriggerScreenFlash(choice.screenFlashColor, choice.screenFlashDuration);
        
        // Modifica spawn rate
        if (choice.spawnRateModifier != 0)
            ModifySpawnRate(choice.spawnRateModifier);
        
        // Dano extra
        if (choice.extraDefenderDamage > 0)
            DealExtraDamage(choice.extraDefenderDamage);
        
        // Animação
        if (!string.IsNullOrEmpty(choice.animationTrigger))
            AnimateSpeaker(choice.animationTrigger);
        
        Debug.Log($"Escolha: {choice.buttonText} | Pressão: {(choice.pressureChange >= 0 ? "+" : "")}{choice.pressureChange}");
        
        EndDialogue();
    }
    
    void ShowReaction(string reaction)
    {
        if (reactionCoroutine != null) StopCoroutine(reactionCoroutine);
        reactionCoroutine = StartCoroutine(DisplayReaction(reaction));
    }
    
    IEnumerator DisplayReaction(string reaction)
    {
        if (reactionPanel != null && reactionText != null)
        {
            reactionText.text = reaction;
            reactionText.alpha = 1f;
            reactionPanel.SetActive(true);
            
            float timer = reactionDisplayTime;
            while (timer > 0)
            {
                timer -= Time.unscaledDeltaTime;
                if (timer < 0.5f)
                    reactionText.alpha = timer / 0.5f;
                yield return null;
            }
            
            reactionPanel.SetActive(false);
        }
    }
    
    void ModifySpawnRate(float modifier)
    {
        EnemyMissileSpawner_Def spawner = Object.FindAnyObjectByType<EnemyMissileSpawner_Def>();
        if (spawner != null)
        {
            spawner.delayBetweenMissiles = Mathf.Max(0.5f, spawner.delayBetweenMissiles - modifier);
        }
    }
    
    void DealExtraDamage(int damageAmount)
    {
        GameObject[] defenders = GameObject.FindGameObjectsWithTag("Defenders");
        if (defenders.Length > 0)
        {
            GameObject target = defenders[Random.Range(0, defenders.Length)];
            CityScript city = target.GetComponent<CityScript>();
            if (city != null)
                city.TakeDamage(damageAmount);
        }
    }
    
    void AnimateSpeaker(string triggerName)
    {
        Animator speakerAnimator = dialoguePanel?.GetComponentInChildren<Animator>();
        if (speakerAnimator != null)
            speakerAnimator.SetTrigger(triggerName);
    }
    
    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;
        
        Time.timeScale = 1f;
        currentDialogue = null;
    }
    
    public bool IsRinging() => isRinging;
    
    public void ResetDialogueSequence()
    {
        scientistCallCount = 0;
        nonScientistCallsSinceLastScientist = 0;
        pendingDialogues.Clear();
    }
}
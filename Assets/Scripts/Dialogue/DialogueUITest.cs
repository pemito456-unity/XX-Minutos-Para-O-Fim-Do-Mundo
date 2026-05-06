using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class DialogueUITest : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float timeBetweenCalls = 10f;
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
    
    [Header("Botões Manuais (4 opções)")]
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Button choiceButton3;
    [SerializeField] private Button choiceButton4;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText1;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText2;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText3;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText4;
    
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
    private float ringingTimer;
    private Coroutine ringingCoroutine;
    
    private int scientistCallCount = 0;
    private int nonScientistCallsSinceLastScientist = 0;
    private const int CALLS_BEFORE_SCIENTIST = 2;
    
    private GameObject flashObject;
    private Image flashImage;
    
    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        
        // Garante que o painel começa DESATIVADO
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("DialoguePanel desativado no Start");
        }
        
        CreateFlashObject();
        HideAllChoiceButtons();
        
        StartCoroutine(DialogueScheduler());
        
        Debug.Log($"=== DialogueUITest iniciado ===");
    }
    
    void Update()
    {
        // Teclas de teste rápido
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
        if (choiceButton4 != null) choiceButton4.gameObject.SetActive(false);
    }
    
    void ShowChoiceButtons(int count)
    {
        HideAllChoiceButtons();
        
        if (count >= 1 && choiceButton1 != null) choiceButton1.gameObject.SetActive(true);
        if (count >= 2 && choiceButton2 != null) choiceButton2.gameObject.SetActive(true);
        if (count >= 3 && choiceButton3 != null) choiceButton3.gameObject.SetActive(true);
        if (count >= 4 && choiceButton4 != null) choiceButton4.gameObject.SetActive(true);
    }
    
    void SetChoiceButtonText(int index, string text)
    {
        switch(index)
        {
            case 1: if (choiceText1 != null) choiceText1.text = text; break;
            case 2: if (choiceText2 != null) choiceText2.text = text; break;
            case 3: if (choiceText3 != null) choiceText3.text = text; break;
            case 4: if (choiceText4 != null) choiceText4.text = text; break;
        }
    }
    
    void ClearAllChoiceListeners()
    {
        if (choiceButton1 != null) choiceButton1.onClick.RemoveAllListeners();
        if (choiceButton2 != null) choiceButton2.onClick.RemoveAllListeners();
        if (choiceButton3 != null) choiceButton3.onClick.RemoveAllListeners();
        if (choiceButton4 != null) choiceButton4.onClick.RemoveAllListeners();
    }
    
    void CreateDialogueButtons(List<DialogueChoice> choices)
    {
        ClearAllChoiceListeners();
        
        int choiceCount = Mathf.Min(choices.Count, 4);
        ShowChoiceButtons(choiceCount);
        
        for (int i = 0; i < choiceCount; i++)
        {
            SetChoiceButtonText(i + 1, choices[i].buttonText);
            
            Button btn = null;
            switch(i)
            {
                case 0: btn = choiceButton1; break;
                case 1: btn = choiceButton2; break;
                case 2: btn = choiceButton3; break;
                case 3: btn = choiceButton4; break;
            }
            
            if (btn != null)
            {
                int index = i;
                btn.onClick.AddListener(() => OnChoiceSelected(choices[index]));
            }
        }
        
        Debug.Log($"✅ {choiceCount} botões de escolha criados");
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
        
        isRinging = true;
        ringingTimer = callRingingDuration;
        
        if (phoneAudioSource != null && phoneRingingClip != null)
        {
            phoneAudioSource.clip = phoneRingingClip;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }
        
        if (ringingCoroutine != null) StopCoroutine(ringingCoroutine);
        ringingCoroutine = StartCoroutine(RingingTimer());
        
        Debug.Log("📞 Telefone tocando...");
        
        // 🔴 CORREÇÃO 1: Quando o telefone toca, automaticamente inicia o diálogo
        // (não precisa de botão para atender)
        AnswerPhone();

        if (phoneAudioSource != null && phoneRingingClip != null)
    {
        phoneAudioSource.clip = phoneRingingClip;
        phoneAudioSource.loop = true;
        phoneAudioSource.Play();
    }
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
            Debug.Log($"⏰ Chamada ignorada! Penalidade de {ignoreCallPenalty}");
            
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
        
        if (phoneAudioSource != null)
        {
            phoneAudioSource.Stop();
            phoneAudioSource.loop = false;
        }
        
        if (ringingCoroutine != null) StopCoroutine(ringingCoroutine);
    }
    
    public void AnswerPhone()
    {
        Debug.Log($"📞 Atendendo chamada... pending={pendingDialogues.Count}");
        
        if (!isDialogueActive && pendingDialogues.Count > 0)
        {
            StopRinging();
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
    
    Debug.Log($"💬 Iniciando diálogo: {currentDialogue.speakerName}");
    
    isDialogueActive = true;
    
    if (gameController != null)
        gameController.currentState = GameController_Def.GameState.Dialogue;
    
    Time.timeScale = 0f;
    
    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(true);
        Debug.Log($"✅ DialoguePanel ativado");
    }
    
    // =============================================
    // 🔴 LINHA ADICIONADA - Toca o telefone uma vez
    // =============================================
    if (phoneAudioSource != null && phoneRingingClip != null)
        phoneAudioSource.PlayOneShot(phoneRingingClip);
    
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
    
    Canvas.ForceUpdateCanvases();
    CreateDialogueButtons(currentDialogue.choices);
    
    Debug.Log($"✅ Diálogo exibido");
}
    
    void OnChoiceSelected(DialogueChoice choice)
    {
        Debug.Log($"🔘 Escolha: {choice.buttonText} | Pressão: {(choice.pressureChange >= 0 ? "+" : "")}{choice.pressureChange}");
        
        if (gameController != null)
        {
            gameController.ModifyPressureByDialogue(choice.pressureChange);
            if (choice.advancesInvestigation)
                gameController.AdvanceInvestigation();
        }
        
        if (choice.screenFlashColor != null && choice.screenFlashColor.a > 0)
            TriggerScreenFlash(choice.screenFlashColor, choice.screenFlashDuration);
        
        EndDialogue();
    }
    
    void EndDialogue()
    {
        Debug.Log($"🔚 Fechando diálogo");
        
        isDialogueActive = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;
        
        Time.timeScale = 1f;
        currentDialogue = null;
        
        HideAllChoiceButtons();
        ClearAllChoiceListeners();
    }
    
    public void TestForceCall()
    {
        Debug.Log("=== TESTE: Forçando chamada ===");
        ScheduleNextDialogue();
    }
    
    public void TestForceDialogue()
    {
        Debug.Log("=== TESTE: Forçando diálogo direto ===");
        
        if (colonelDialogues.Count > 0)
        {
            currentDialogue = colonelDialogues[0];
            StartDialogue();
        }
        else
        {
            Debug.LogError("Nenhum diálogo do Coronel disponível para teste!");
        }
    }
    
    public void ResetDialogueSequence()
    {
        scientistCallCount = 0;
        nonScientistCallsSinceLastScientist = 0;
        pendingDialogues.Clear();
        Debug.Log("Sequência de diálogos resetada!");
    }
    
    public bool IsRinging() => isRinging;
}
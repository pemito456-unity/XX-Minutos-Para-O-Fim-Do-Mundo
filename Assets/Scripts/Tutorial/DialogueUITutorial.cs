using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueUITutorial : MonoBehaviour
{
    [Header("UI (mesma estrutura da cena principal)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private TMPro.TextMeshProUGUI speakerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    
    [Header("Botões (3 opções - mesma da cena principal)")]
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Button choiceButton3;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText1;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText2;
    [SerializeField] private TMPro.TextMeshProUGUI choiceText3;
    
    [Header("Configurações")]
    [SerializeField] private float reactionTime = 2f;
    
    private DialogueData currentDialogue;
    private bool isDialogueActive = false;
    private System.Action onDialogueComplete;
    
    void Start()
    {
        // 🔴 NÃO desativa o painel aqui!
        // Apenas garante que os botões estão configurados
        SetupButtons();
        Debug.Log("DialogueUITutorial iniciado!");
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
        
        Debug.Log("Botões do tutorial configurados!");
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

        for (int i = 0; i < choiceCount; i++)
        {
            SetChoiceButtonText(i + 1, choices[i].buttonText);
            DialogueButtonStyling.ApplyChoiceButtonHover(buttons[i]);
        }

        Debug.Log($"Tutorial: {choiceCount} botões configurados com hover");
    }
    
    public void ShowDialogue(DialogueData dialogue, System.Action onComplete = null)
    {
        Debug.Log($"ShowDialogue chamado para: {dialogue.speakerName}");
        
        if (dialogue == null)
        {
            Debug.LogError("Diálogo é null!");
            if (onComplete != null) onComplete.Invoke();
            return;
        }
        
        currentDialogue = dialogue;
        onDialogueComplete = onComplete;
        
        // 🔴 ATIVA O PAINEL ANTES DE INICIAR A CORROTINA
        if (dialoguePanel != null && !dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("DialoguePanel ativado antes da corrotina");
        }
        
        StartCoroutine(StartDialogueCoroutine());
    }
    
    IEnumerator StartDialogueCoroutine()
    {
        isDialogueActive = true;
        
        // 🔴 GARANTE QUE O PAINEL ESTÁ ATIVO
        if (dialoguePanel != null && !dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("DialoguePanel ativado na corrotina");
        }
        
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
    
    public void OnButtonClick(int buttonIndex)
    {
        if (!isDialogueActive) return;
        
        Debug.Log($"🔘 Tutorial - Botão {buttonIndex + 1} clicado!");
        
        if (currentDialogue != null && buttonIndex < currentDialogue.choices.Count)
        {
            StartCoroutine(ShowReactionAndClose(currentDialogue.choices[buttonIndex]));
        }
    }
    
    IEnumerator ShowReactionAndClose(DialogueChoice choice)
    {
        HideAllChoiceButtons();
        
        if (!string.IsNullOrEmpty(choice.speakerReaction))
        {
            if (dialogueText != null)
                dialogueText.text = choice.speakerReaction;
            Debug.Log($"🎭 Reação: {choice.speakerReaction}");
        }
        
        yield return new WaitForSecondsRealtime(reactionTime);
        
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
    
    public bool IsDialogueActive() => isDialogueActive;
}
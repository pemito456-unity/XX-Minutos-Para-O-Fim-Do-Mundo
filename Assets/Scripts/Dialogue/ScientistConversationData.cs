using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScientistConversation", menuName = "Dialogue/Scientist Conversation")]
public class ScientistConversationData : ScriptableObject
{
    [Header("Informações do Cientista")]
    public string speakerName = "Cientista";
    public Sprite speakerPortrait;

    [Header("Configurações")]
    public float timeToDecidePerResponse = 20f;
    public bool advancesInvestigationOnComplete = true;

    [Header("Conversa (alterna: fala do cientista → resposta do jogador)")]
    public List<ConversationExchange> exchanges = new List<ConversationExchange>();

    [Header("Fala final do cientista (opcional)")]
    [TextArea(2, 5)]
    public string closingScientistLine;

    [Header("Áudio (opcional)")]
    public AudioClip dialogueStartClip;
}

[System.Serializable]
public class ConversationExchange
{
    [Header("Fala do Cientista")]
    [TextArea(3, 6)]
    public string scientistLine;

    [Header("Resposta do Jogador (única opção)")]
    public ConversationPlayerChoice playerResponse = new ConversationPlayerChoice();
}

[System.Serializable]
public class ConversationPlayerChoice
{
    [TextArea(1, 3)]
    public string buttonText;

    public float pressureChange;
    public bool advancesInvestigation;
}

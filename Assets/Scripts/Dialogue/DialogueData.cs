using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Informações do Locutor")]
    public string speakerName;
    public SpeakerType speakerType;
    public Sprite speakerPortrait;
    
    [Header("Texto do Diálogo")]
    [TextArea(3, 5)]
    public string dialogueText;
    
    [Header("Configurações")]
    public bool isMandatory; // Diálogos do cientista são obrigatórios
    public float timeToDecide = 15f; // Tempo máximo para responder (segundos)
    
    [Header("Opções de Resposta (Botões)")]
    public List<DialogueChoice> choices;
    
    [Header("Áudio (opcional)")]
    public AudioClip dialogueStartClip; // Áudio que toca quando o diálogo começa
    public AudioClip speakerVoiceClip;   // Voz do locutor (opcional)
}

public enum SpeakerType
{
    Colonel,      // Coronel - questões militares
    Secretary,    // Secretário de Defesa - questões civis/nacionais
    Scientist     // Cientista - progresso da investigação
}

[System.Serializable]
public class DialogueChoice
{
    [Header("Texto do Botão")]
    [TextArea(1, 2)]
    public string buttonText; // O texto que o jogador vê no botão (a resposta dele)
    
    [Header("Consequências")]
    public float pressureChange;  // Positivo = aumenta pressão, Negativo = diminui
    public bool advancesInvestigation; // Apenas para o cientista
    
    [Header("Reação do Interlocutor")]
    [TextArea(2, 4)]
    public string speakerReaction; // Como o interlocutor REAGE à escolha do jogador
    
    [Header("Efeitos Visuais/Sonoros")]
    public string animationTrigger; // Nome da animação (ex: "Angry", "Calm", "Panic")
    public AudioClip reactionSound;  // Som específico para esta reação
    public Color screenFlashColor;   // Cor do flash na tela (ex: vermelho para respostas ruins)
    public float screenFlashDuration = 0.2f;
    
    [Header("Efeitos no Mundo do Jogo")]
    public float spawnRateModifier = 0f; // Altera velocidade de spawn dos meteoros
    public int extraDefenderDamage = 0;  // Dano extra aos defensores (resposta ruim)
}
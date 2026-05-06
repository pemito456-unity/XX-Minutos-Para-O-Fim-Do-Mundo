using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI computerText;
    [SerializeField] private TextMeshProUGUI alastairText;
    
    [Header("Configurações")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float delayBetweenLines = 1f;
    
    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingSound;
    
    [Header("Próxima Cena")]
    [SerializeField] private string nextSceneName = "MainGame";
    
    private string[] computerLines = new string[]
    {
        "ATENÇÃO. MÚLTIPLOS OBJETOS FORAM DETECTADOS EM TRAJETÓRIA BALÍSTICA.",
        "DISTÂNCIA MÉDIA: 2515 KM AO NORTE-NOROESTE DA CAPITAL.",
        "VELOCIDADE MÉDIA: 18 KM/S.",
        "INCLINAÇÃO MÉDIA DA TRAJETÓRIA: 44 GRAUS.",
        "TEMPO ESTIMADO ATÉ O PRIMEIRO IMPACTO: 5 MINUTOS E 19 SEGUNDOS.",
        "PREPARE AS BATERIAS ANTIMÍSSEIS IMEDIATAMENTE."
    };
    
    private string[] alastairLines = new string[]
    {
        "Três mil quilômetros de altitude?",
        "Isso é quase o dobro do que mísseis intercontinentais...",
        "18 quilômetros por segundo? Isso é absurdo!"
    };
    
    void Start()
    {
        StartCoroutine(RunIntro());
    }
    
    IEnumerator RunIntro()
    {
        // Ativa o painel preto
        if (introPanel != null)
            introPanel.SetActive(true);
        
        // Limpa os textos
        if (computerText != null)
            computerText.text = "";
        if (alastairText != null)
            alastairText.text = "";
        
        // Mostra as linhas do computador
        foreach (string line in computerLines)
        {
            yield return StartCoroutine(TypeLine(computerText, line));
            yield return new WaitForSeconds(delayBetweenLines);
        }
        
        // Pausa antes de Alastair
        yield return new WaitForSeconds(0.5f);
        
        // Mostra as linhas de Alastair
        foreach (string line in alastairLines)
        {
            yield return StartCoroutine(TypeLine(alastairText, line));
            yield return new WaitForSeconds(delayBetweenLines * 0.8f);
        }
        
        // Aguarda e carrega a próxima cena
        yield return new WaitForSeconds(2f);
        
        if (!string.IsNullOrEmpty(nextSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
    
    IEnumerator TypeLine(TextMeshProUGUI textComponent, string line)
    {
        textComponent.text = "";
        
        for (int i = 0; i <= line.Length; i++)
        {
            textComponent.text = line.Substring(0, i);
            
            // Toca o som de digitação a cada letra
            if (typingSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(typingSound, 0.3f);
            }
            
            yield return new WaitForSeconds(typeSpeed);
        }
        
        // Pequena pausa no final da linha
        yield return new WaitForSeconds(0.2f);
    }
}
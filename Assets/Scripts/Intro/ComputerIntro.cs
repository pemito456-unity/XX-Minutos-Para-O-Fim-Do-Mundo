using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using TMPro;

public class ComputerIntro : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float delayBetweenLines = 1f;
    
    [Header("Áudio")]
    [Tooltip("Som curto de tecla (recomendado < 0,3s). Clipes longos só tocam de novo após terminar.")]
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private float soundVolume = 0.1f;
    
    [Header("Próxima Cena")]
    [SerializeField] private string nextSceneName = "MainGame";
    
    [Header("Fontes")]
    [SerializeField] private TMP_FontAsset computerFont;
    [SerializeField] private Font alastairFont;
    
    private AudioSource audioSource;
    private TextMeshProUGUI computerText;
    private Text alastairText;
    private GameObject introPanel;
    private float nextTypingSoundAllowedTime;
    
    private StringBuilder computerFullText = new StringBuilder();
    private StringBuilder alastairFullText = new StringBuilder();
    
    private bool skipRequested = false;
    private Coroutine currentRoutine;
    
    // 🔴 NOVO TEXTO DO COMPUTADOR ATUALIZADO
    private string[] computerLines = new string[]
    {
        "Atenção: múltiplos objetos não identificados detectados em reentrada atmosférica.",
        "Alvos principais: região metropolitana de Washington e arredores, área estimada de 32.720 km².",
        "Distância atual: aproximadamente 1.680 quilômetros de altitude ao norte-noroeste da capital. Ângulo de incidência médio de 75 graus.",
        "Velocidade: 17,9 km/s.",
        "Tempo estimado até o primeiro impacto em Washington D.C.: 6 minutos e 12 segundos.",
        "Prepare todas as baterias antimísseis imediatamente. Este não é um exercício."
    };
    
    private string[] alastairLines = new string[]
    {
        "Três mil quilômetros de altitude?",
        "Isso é quase o dobro do que mísseis intercontinentais poderiam ser capazes...",
        "Não é impossível, mas a 18 quilômetros por segundo?",
        "Isso é absurdo!"
    };
    
    void Awake()
    {
        SetupAudioSource();
    }

    void Start()
    {
        CreateUI();
        currentRoutine = StartCoroutine(RunIntro());
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = null;
        audioSource.volume = soundVolume;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !skipRequested)
        {
            SkipIntro();
        }
    }
    
    private void SkipIntro()
    {
        skipRequested = true;
        
        Debug.Log("🚀 Pular introdução solicitado (Tecla E)! Carregando cena principal...");
        
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        if (audioSource != null)
            audioSource.Stop();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            introPanel.SetActive(false);
        }
    }
    
    void CreateUI()
    {
        GameObject canvasGO = new GameObject("IntroCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        introPanel = new GameObject("IntroPanel");
        introPanel.transform.SetParent(canvasGO.transform, false);
        
        RectTransform panelRect = introPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = introPanel.AddComponent<Image>();
        panelImage.color = Color.black;
        
        GameObject computerGO = new GameObject("ComputerText");
        computerGO.transform.SetParent(introPanel.transform, false);
        
        computerText = computerGO.AddComponent<TextMeshProUGUI>();
        if (computerFont != null)
        {
            computerText.font = computerFont;
            Debug.Log("Fonte VT232 aplicada ao computador!");
        }
        else
        {
            Debug.LogWarning("Fonte VT232 não atribuída! Usando fonte padrão TMP.");
        }
        computerText.fontSize = 38;
        computerText.color = new Color(0, 0.9f, 0);
        computerText.alignment = TextAlignmentOptions.TopLeft;
        computerText.enableWordWrapping = true;
        
        RectTransform computerRect = computerGO.GetComponent<RectTransform>();
        computerRect.anchorMin = new Vector2(0, 0.5f);
        computerRect.anchorMax = new Vector2(1, 0.9f);
        computerRect.offsetMin = new Vector2(100, 0);
        computerRect.offsetMax = new Vector2(-100, -20);
        
        GameObject alastairGO = new GameObject("AlastairText");
        alastairGO.transform.SetParent(introPanel.transform, false);
        
        alastairText = alastairGO.AddComponent<Text>();
        if (alastairFont != null)
            alastairText.font = alastairFont;
        else
            alastairText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        alastairText.fontSize = 30;
        alastairText.color = Color.white;
        alastairText.alignment = TextAnchor.UpperLeft;
        alastairText.horizontalOverflow = HorizontalWrapMode.Wrap;
        alastairText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform alastairRect = alastairGO.GetComponent<RectTransform>();
        alastairRect.anchorMin = new Vector2(0, 0);
        alastairRect.anchorMax = new Vector2(1, 0.4f);
        alastairRect.offsetMin = new Vector2(100, 50);
        alastairRect.offsetMax = new Vector2(-100, -20);
        
        GameObject skipTextGO = new GameObject("SkipText");
        skipTextGO.transform.SetParent(introPanel.transform, false);
        
        Text skipText = skipTextGO.AddComponent<Text>();
        skipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        skipText.fontSize = 24;
        skipText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        skipText.text = "Pressione E para pular";
        skipText.alignment = TextAnchor.LowerRight;
        
        RectTransform skipRect = skipTextGO.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0, 0);
        skipRect.anchorMax = new Vector2(1, 0);
        skipRect.pivot = new Vector2(1, 0);
        skipRect.offsetMin = new Vector2(0, 20);
        skipRect.offsetMax = new Vector2(-30, 60);
        
        Debug.Log("UI criada com sucesso!");
    }
    
    IEnumerator RunIntro()
    {
        computerFullText.Clear();
        alastairFullText.Clear();
        computerText.text = "";
        alastairText.text = "";
        
        yield return new WaitForSeconds(0.5f);
        
        for (int i = 0; i < computerLines.Length; i++)
        {
            if (skipRequested) yield break;
            
            if (i > 0)
            {
                computerFullText.Append("\n\n");
            }
            
            computerFullText.Append(computerLines[i]);
            
            yield return StartCoroutine(TypeLineTMP(computerText, computerFullText.ToString(), computerLines[i], playTypingSound: true));
            StopIntroAudio();

            if (!skipRequested)
                yield return new WaitForSeconds(delayBetweenLines);
        }
        
        if (skipRequested) yield break;

        StopIntroAudio();
        yield return new WaitForSeconds(0.8f);
        
        for (int i = 0; i < alastairLines.Length; i++)
        {
            if (skipRequested) yield break;
            
            if (i > 0)
            {
                alastairFullText.Append("\n\n");
            }
            
            alastairFullText.Append(alastairLines[i]);
            
            yield return StartCoroutine(TypeLine(alastairText, alastairFullText.ToString(), alastairLines[i], playTypingSound: false));
            
            if (!skipRequested)
                yield return new WaitForSeconds(delayBetweenLines * 0.7f);
        }
        
        if (skipRequested) yield break;
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Introdução finalizada!");
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            introPanel.SetActive(false);
        }
    }
    
    IEnumerator TypeLineTMP(TextMeshProUGUI textComponent, string fullText, string newLine, bool playTypingSound)
    {
        string existingText = fullText.Substring(0, fullText.Length - newLine.Length);
        
        for (int i = 0; i <= newLine.Length; i++)
        {
            if (skipRequested)
            {
                textComponent.text = fullText;
                yield break;
            }
            
            string typedPart = newLine.Substring(0, i);
            textComponent.text = existingText + typedPart;
            
            if (playTypingSound && i % 2 == 0 && i > 0)
                TryPlayTypingSound();

            yield return new WaitForSeconds(typeSpeed);
        }
        
        yield return new WaitForSeconds(0.2f);
    }
    
    IEnumerator TypeLine(Text textComponent, string fullText, string newLine, bool playTypingSound)
    {
        string existingText = fullText.Substring(0, fullText.Length - newLine.Length);
        
        for (int i = 0; i <= newLine.Length; i++)
        {
            if (skipRequested)
            {
                textComponent.text = fullText;
                yield break;
            }
            
            string typedPart = newLine.Substring(0, i);
            textComponent.text = existingText + typedPart;
            
            if (playTypingSound && i % 2 == 0 && i > 0)
                TryPlayTypingSound();

            yield return new WaitForSeconds(typeSpeed);
        }
        
        yield return new WaitForSeconds(0.2f);
    }

    void TryPlayTypingSound()
    {
        if (typingSound == null || audioSource == null)
            return;

        float now = Time.unscaledTime;
        if (now < nextTypingSoundAllowedTime)
            return;

        float minGap = Mathf.Max(0.1f, typingSound.length);
        nextTypingSoundAllowedTime = now + minGap;
        audioSource.PlayOneShot(typingSound, soundVolume);
    }

    void StopIntroAudio()
    {
        if (audioSource != null)
            audioSource.Stop();

        nextTypingSoundAllowedTime = 0f;
    }
}
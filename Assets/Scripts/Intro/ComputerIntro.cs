using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class ComputerIntro : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float delayBetweenLines = 1f;
    
    [Header("Áudio")]
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private float soundVolume = 0.1f;
    
    [Header("Próxima Cena")]
    [SerializeField] private string nextSceneName = "MainGame";
    
    private AudioSource audioSource;
    private Text computerText;
    private Text alastairText;
    private GameObject introPanel;
    
    // Acumuladores de texto
    private StringBuilder computerFullText = new StringBuilder();
    private StringBuilder alastairFullText = new StringBuilder();
    
    // 🔴 CONTROLE DE SKIP
    private bool skipRequested = false;
    private Coroutine currentRoutine;
    
    private string[] computerLines = new string[]
    {
        "ATENÇÃO. MÚLTIPLOS OBJETOS FORAM DETECTADOS EM TRAJETÓRIA BALÍSTICA.",
        "DISTÂNCIA MÉDIA: 2515 KM AO NORTE-NOROESTE DA CAPITAL, A 2914 KM DE ALTITUDE.",
        "VELOCIDADE MÉDIA: 18 KM/S.",
        "INCLINAÇÃO MÉDIA DA TRAJETÓRIA EM RELAÇÃO À SUPERFÍCIE: 44 GRAUS.",
        "TEMPO ESTIMADO ATÉ O PRIMEIRO IMPACTO: 5 MINUTOS E 19 SEGUNDOS.",
        "OUTROS OBJETOS SEGUEM EM FORMAÇÃO. PREPARE AS BATERIAS ANTIMÍSSEIS IMEDIATAMENTE."
    };
    
    private string[] alastairLines = new string[]
    {
        "Três mil quilômetros de altitude?",
        "Isso é quase o dobro do que mísseis intercontinentais poderiam ser capazes...",
        "Não é impossível, mas a 18 quilômetros por segundo?",
        "Isso é absurdo!"
    };
    
    void Start()
    {
        CreateUI();
        currentRoutine = StartCoroutine(RunIntro());
    }
    
    void Update()
    {
        // 🔴 TECLA E PARA PULAR A CUTSCENE (carrega a cena diretamente)
        if (Input.GetKeyDown(KeyCode.E) && !skipRequested)
        {
            SkipIntro();
        }
    }
    
    // 🔴 MÉTODO PARA PULAR A INTRODUÇÃO
    private void SkipIntro()
    {
        skipRequested = true;
        
        Debug.Log("🚀 Pular introdução solicitado (Tecla E)! Carregando cena principal...");
        
        // Para a corrotina atual
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        
        // Carrega a próxima cena diretamente (sem fade)
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
        // 1. Cria o Canvas
        GameObject canvasGO = new GameObject("IntroCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // 2. Cria o painel preto
        introPanel = new GameObject("IntroPanel");
        introPanel.transform.SetParent(canvasGO.transform, false);
        
        RectTransform panelRect = introPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = introPanel.AddComponent<Image>();
        panelImage.color = Color.black;
        
        // 3. Cria o texto do computador
        GameObject computerGO = new GameObject("ComputerText");
        computerGO.transform.SetParent(introPanel.transform, false);
        
        computerText = computerGO.AddComponent<Text>();
        computerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        computerText.fontSize = 34;
        computerText.color = new Color(0, 0.8f, 0);
        computerText.alignment = TextAnchor.UpperLeft;
        computerText.horizontalOverflow = HorizontalWrapMode.Wrap;
        computerText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform computerRect = computerGO.GetComponent<RectTransform>();
        computerRect.anchorMin = new Vector2(0, 0.5f);
        computerRect.anchorMax = new Vector2(1, 0.9f);
        computerRect.offsetMin = new Vector2(100, 0);
        computerRect.offsetMax = new Vector2(-100, -20);
        
        // 4. Cria o texto de Alastair
        GameObject alastairGO = new GameObject("AlastairText");
        alastairGO.transform.SetParent(introPanel.transform, false);
        
        alastairText = alastairGO.AddComponent<Text>();
        alastairText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        alastairText.fontSize = 34;
        alastairText.color = Color.white;
        alastairText.alignment = TextAnchor.UpperLeft;
        alastairText.horizontalOverflow = HorizontalWrapMode.Wrap;
        alastairText.verticalOverflow = VerticalWrapMode.Overflow;
        
        RectTransform alastairRect = alastairGO.GetComponent<RectTransform>();
        alastairRect.anchorMin = new Vector2(0, 0);
        alastairRect.anchorMax = new Vector2(1, 0.4f);
        alastairRect.offsetMin = new Vector2(100, 50);
        alastairRect.offsetMax = new Vector2(-100, -20);
        
        // 🔴 TEXTO "Pressione E para pular" (canto inferior direito)
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
        
        // 5. Cria o AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        
        Debug.Log("UI criada com sucesso!");
    }
    
    IEnumerator RunIntro()
    {
        // Limpa os textos e acumuladores
        computerFullText.Clear();
        alastairFullText.Clear();
        computerText.text = "";
        alastairText.text = "";
        
        yield return new WaitForSeconds(0.5f);
        
        // 🔴 COMPUTADOR - Acumula frases
        for (int i = 0; i < computerLines.Length; i++)
        {
            if (skipRequested) yield break;
            
            // Adiciona quebra de linha se não for a primeira frase
            if (i > 0)
            {
                computerFullText.Append("\n\n");
            }
            
            computerFullText.Append(computerLines[i]);
            
            // Digita a frase atual (mantendo as anteriores)
            yield return StartCoroutine(TypeLine(computerText, computerFullText.ToString(), computerLines[i]));
            
            if (!skipRequested)
                yield return new WaitForSeconds(delayBetweenLines);
        }
        
        if (skipRequested) yield break;
        
        yield return new WaitForSeconds(0.8f);
        
        // 🔴 ALASTAIR - Acumula frases
        for (int i = 0; i < alastairLines.Length; i++)
        {
            if (skipRequested) yield break;
            
            if (i > 0)
            {
                alastairFullText.Append("\n\n");
            }
            
            alastairFullText.Append(alastairLines[i]);
            
            yield return StartCoroutine(TypeLine(alastairText, alastairFullText.ToString(), alastairLines[i]));
            
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
    
    IEnumerator TypeLine(Text textComponent, string fullText, string newLine)
    {
        // Calcula o texto que já estava lá (sem a nova linha)
        string existingText = fullText.Substring(0, fullText.Length - newLine.Length);
        
        // Digita a nova linha caractere por caractere
        for (int i = 0; i <= newLine.Length; i++)
        {
            // 🔴 VERIFICA SE PULAR FOI SOLICITADO
            if (skipRequested)
            {
                textComponent.text = fullText;
                yield break;
            }
            
            string typedPart = newLine.Substring(0, i);
            textComponent.text = existingText + typedPart;
            
            // Toca som de digitação (a cada 2 letras)
            if (typingSound != null && audioSource != null && i % 2 == 0 && i > 0)
            {
                audioSource.PlayOneShot(typingSound, soundVolume);
            }
            
            yield return new WaitForSeconds(typeSpeed);
        }
        
        yield return new WaitForSeconds(0.2f);
    }
}
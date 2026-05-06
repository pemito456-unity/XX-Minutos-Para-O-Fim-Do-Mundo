using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueFeedback : MonoBehaviour
{
    private static DialogueFeedback instance;
    
    [Header("Referência do Canvas")]
    [SerializeField] private Canvas targetCanvas;
    
    private GameObject flashObject;
    private Image flashImage;
    
    void Awake()
    {
        // Configura a instância singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        CreateFlashObject();
    }
    
    void CreateFlashObject()
    {
        // Procura o canvas se não foi atribuído
        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
            
            // Se ainda não encontrar, cria um canvas
            if (targetCanvas == null)
            {
                GameObject canvasGO = new GameObject("TempCanvas");
                targetCanvas = canvasGO.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                DontDestroyOnLoad(canvasGO);
                Debug.Log("DialogueFeedback: Canvas temporário criado.");
            }
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("DialogueFeedback: Não foi possível encontrar ou criar um Canvas!");
            return;
        }
        
        // Cria o objeto de flash
        flashObject = new GameObject("ScreenFlash");
        flashObject.transform.SetParent(targetCanvas.transform, false);
        
        // Adiciona a imagem
        flashImage = flashObject.AddComponent<Image>();
        flashImage.raycastTarget = false;
        flashImage.color = new Color(0, 0, 0, 0);
        
        // Configura o rect transform para ocupar a tela toda
        RectTransform rect = flashObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Garante que fique na frente de tudo
        rect.SetAsLastSibling();
        
        flashObject.SetActive(false);
        
        Debug.Log("DialogueFeedback: Sistema de flash inicializado.");
    }
    
    public static void TriggerScreenFlash(Color color, float duration)
    {
        if (instance == null)
        {
            Debug.LogWarning("DialogueFeedback: Instância não encontrada. Criando uma nova...");
            GameObject go = new GameObject("DialogueFeedback");
            instance = go.AddComponent<DialogueFeedback>();
        }
        
        if (instance.flashImage == null)
        {
            instance.CreateFlashObject();
        }
        
        if (instance.flashImage != null)
        {
            instance.StartCoroutine(instance.ScreenFlashCoroutine(color, duration));
        }
        else
        {
            Debug.LogWarning("DialogueFeedback: Não foi possível criar o efeito de flash.");
        }
    }
    
    IEnumerator ScreenFlashCoroutine(Color color, float duration)
    {
        if (flashObject == null) yield break;
        
        flashObject.SetActive(true);
        
        // Cor inicial (alpha 0.7 para ser bem visível)
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
    
    // Método para flashes pré-definidos
    public static void FlashRed(float duration = 0.3f)
    {
        TriggerScreenFlash(Color.red, duration);
    }
    
    public static void FlashGreen(float duration = 0.3f)
    {
        TriggerScreenFlash(Color.green, duration);
    }
    
    public static void FlashYellow(float duration = 0.3f)
    {
        TriggerScreenFlash(Color.yellow, duration);
    }
    
    public static void FlashWhite(float duration = 0.3f)
    {
        TriggerScreenFlash(Color.white, duration);
    }
    
    // Limpeza ao destruir
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
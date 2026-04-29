using UnityEngine;
using UnityEngine.UI;

public class PressureBarSystem : MonoBehaviour
{
    [Header("Configuração da Barra")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform ballIcon;
    
    [Header("Limites da Bolinha (X)")]
    [SerializeField] private float minX = -71.9f;
    [SerializeField] private float maxX = 248f;
    
    private GameController_Def gameController;
    private RectTransform barRect;

    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        barRect = fillImage.GetComponent<RectTransform>();
        Debug.Log("PressureBarSystem iniciado");
    }

    void Update()
    {
        if (gameController != null && fillImage != null && ballIcon != null)
        {
            float pressurePercent = gameController.GetPressurePercent();
            
            // Atualiza o preenchimento da barra
            fillImage.fillAmount = pressurePercent;
            
            // Calcula a posição X da bolinha baseada na porcentagem
            // pressurePercent = 0 → minX, pressurePercent = 1 → maxX
            float newX = Mathf.Lerp(minX, maxX, pressurePercent);
            
            // Aplica a posição (mantém o Y original)
            ballIcon.anchoredPosition = new Vector2(newX, ballIcon.anchoredPosition.y);
            
            // Debug opcional (pode remover depois)
            // Debug.Log($"Pressão: {pressurePercent * 100}%, Posição X: {newX}");
        }
    }
}
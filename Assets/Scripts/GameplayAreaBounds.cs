using UnityEngine;

/// <summary>
/// Define a área jogável (laterais da tela, excluindo a faixa central da UI).
/// Usado pelo spawner de meteoros e pelo controle de disparo.
/// </summary>
public class GameplayAreaBounds : MonoBehaviour
{
    [Header("Área de Exclusão (Interface)")]
    [SerializeField] private float excludeXMin = -8.36f;
    [SerializeField] private float excludeXMax = -3.83f;

    [Header("Margem de Segurança")]
    [SerializeField] private float safeMargin = 0.2f;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    public float ExcludeXMin => excludeXMin;
    public float ExcludeXMax => excludeXMax;
    public float SafeMargin => safeMargin;
    public float MinX => minX;
    public float MaxX => maxX;

    void Awake()
    {
        RefreshScreenBounds();
    }

    public void RefreshScreenBounds()
    {
        if (Camera.main == null)
            return;

        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        minX = bottomLeft.x;
        maxX = topRight.x;
        minY = bottomLeft.y;
        maxY = topRight.y;
    }

    public bool IsXInForbiddenArea(float x)
    {
        float forbiddenStart = excludeXMin - safeMargin;
        float forbiddenEnd = excludeXMax + safeMargin;
        return x >= forbiddenStart && x <= forbiddenEnd;
    }

    public bool IsWorldPositionInPlayableArea(Vector2 worldPosition)
    {
        if (worldPosition.x < minX || worldPosition.x > maxX)
            return false;

        if (worldPosition.y < minY || worldPosition.y > maxY)
            return false;

        return !IsXInForbiddenArea(worldPosition.x);
    }

    public static GameplayAreaBounds FindInScene()
    {
        return Object.FindAnyObjectByType<GameplayAreaBounds>();
    }
}

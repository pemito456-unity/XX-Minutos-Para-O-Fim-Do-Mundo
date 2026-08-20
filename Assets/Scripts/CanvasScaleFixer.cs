using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Garante escala consistente do Canvas em qualquer proporção de tela (Free Aspect).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasScaler))]
public class CanvasScaleFixer : MonoBehaviour
{
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField] [Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

    private CanvasScaler scaler;
    private Vector2 lastScreenSize;

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        Apply();
    }

    void Update()
    {
        Vector2 current = new Vector2(Screen.width, Screen.height);
        if (current != lastScreenSize)
        {
            lastScreenSize = current;
            Apply();
        }
    }

    void Apply()
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
    }
}

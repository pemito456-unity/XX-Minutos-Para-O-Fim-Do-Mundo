using UnityEngine;

/// <summary>
/// Mantém a área de jogo (câmera + SceneImages) alinhada com o overlay da UI em qualquer aspect ratio.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class GameAspectAdapter : MonoBehaviour
{
    [SerializeField] private float referenceAspect = 16f / 9f;
    [SerializeField] private Transform sceneImages;
    [SerializeField] private float baseSceneImagesScale = 0.7f;

    private Camera cam;
    private float baseOrthographicSize;
    private Vector2 lastScreenSize;

    void Awake()
    {
        cam = GetComponent<Camera>();
        baseOrthographicSize = cam.orthographicSize;
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
        float windowAspect = (float)Screen.width / Screen.height;
        float scale = windowAspect / referenceAspect;

        if (scale < 1f)
            cam.orthographicSize = baseOrthographicSize / scale;
        else
            cam.orthographicSize = baseOrthographicSize;

        if (sceneImages != null)
        {
            float orthoRatio = cam.orthographicSize / baseOrthographicSize;
            float s = baseSceneImagesScale * orthoRatio;
            sceneImages.localScale = new Vector3(s, s, s);
        }
    }
}

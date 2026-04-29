using UnityEngine;

public class CursorController_Def : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private GameObject missileLauncherPrefab;
    
    [SerializeField] private Texture2D cursorTexture;
    private Vector2 cursorHotspot;

    private GameController_Def myGameController;

    void Start()
    {
        myGameController = Object.FindAnyObjectByType<GameController_Def>();
        
        // Configuração do Cursor Personalizado
        if (cursorTexture != null)
        {
            cursorHotspot = new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f);
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
    }

    void Update()
    {
        // SÓ ATIRA SE: 
        // 1. Clicar com o botão esquerdo
        // 2. O jogo estiver no estado de Gameplay (não atira durante diálogo ou pause)
        if (Input.GetMouseButtonDown(0) && myGameController.currentState == GameController_Def.GameState.Gameplay)
        {
            AtirarMíssil();
        }
    }

    void AtirarMíssil()
    {
        if (missilePrefab != null && missileLauncherPrefab != null)
        {
            Instantiate(missilePrefab, missileLauncherPrefab.transform.position, Quaternion.identity);
            
            // Se você ainda quiser que o tiro custe "Pressão" ou algo assim, adicione aqui.
            // Por enquanto, apenas instanciamos o projétil de defesa.
        }
    }
}
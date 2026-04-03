using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private GameObject MissilePrefab;
    [SerializeField] private GameObject MissileLauncherPrefab;
    
    [SerializeField] private Texture2D cursorTexture;
    private Vector2 cursorHotspot;
    
    private GameController myGameController;
    
    

    void Start()
    { 
            myGameController = GameObject.FindObjectOfType<GameController>(); 
        
            cursorHotspot = new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f);
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && myGameController.playerMissilesLeft > 0)
        {
            Instantiate (MissilePrefab, MissileLauncherPrefab.transform.position, Quaternion.identity);
            myGameController.playerMissilesLeft--;
            myGameController.UpdateMissilesLeftText();
        }
    }
}
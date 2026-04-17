using UnityEngine;

public class CursorController_Def : MonoBehaviour
{

    [SerializeField] GameObject missilePrefab;
    [SerializeField] GameObject missileLauncherPrefab;
    
    [SerializeField] private Texture2D cursorTexture;
    private Vector2 CursorHotspot;


    private GameController_Def myGameController;

    void Start()
    {
        myGameController = GameObject.FindObjectOfType<GameController_Def>();
        CursorHotspot = new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f);
        Cursor.SetCursor(cursorTexture, CursorHotspot, CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && myGameController.playerMissilesLeft > 0)
        {
            Instantiate(missilePrefab, missileLauncherPrefab.transform.position, Quaternion.identity);
            myGameController.playerMissilesLeft--;
            myGameController.UpdateMissilesLeftText();
        }
    }
}

using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private GameObject PlayerMissilePrefab;
    [SerializeField] private GameObject missileLauncher;

    [SerializeField] private Texture2D cursorTexture;
    private Vector2 cursorHotspot;

    void Start()
    {
        cursorHotspot = new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShootMissile();
        }
    }

    void ShootMissile()
    {
        if (missileLauncher == null)
        {
            Debug.LogError("Launcher não atribuído!");
            return;
        }

        Vector3 spawnPos = missileLauncher.transform.position;
        spawnPos.z = 0f;

        Debug.Log("Spawn REAL: " + spawnPos);

        Instantiate(PlayerMissilePrefab, new Vector3(999, 999, 0), Quaternion.identity);
    }
}
using UnityEngine;

public class CursorController_Def : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Texture2D cursorTexture;
    
    [Header("Posição do Canhão")]
    [SerializeField] private Vector2 cannonPosition = new Vector2(2.53f, -3.69f);
    
    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 0.5f;
    
    [Header("Cooldown")]
    [SerializeField] private float shootCooldown = 0.5f;
    private float lastShootTime = -1f;

    private GameController_Def myGameController;

    [System.Obsolete]
    void Start()
    {
        myGameController = FindObjectOfType<GameController_Def>();
        
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (missilePrefab == null) 
            Debug.LogError("MissilePrefab não atribuído!");
        
        Debug.Log($"CursorController iniciado. Posição do canhão: {cannonPosition}");
    }

    void Update()
    {
        if (myGameController != null && myGameController.currentState != GameController_Def.GameState.Gameplay)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        if (missilePrefab != null)
        {
            Vector3 spawnPosition = new Vector3(cannonPosition.x, cannonPosition.y, 0);
            GameObject newMissile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Míssil disparado da posição: {spawnPosition}");
            
            if (audioSource && shootSound)
            {
                audioSource.PlayOneShot(shootSound, shootVolume);
            }
        }
        else
        {
            Debug.LogError("Falha ao atirar: missilePrefab é null!");
        }
    }
}
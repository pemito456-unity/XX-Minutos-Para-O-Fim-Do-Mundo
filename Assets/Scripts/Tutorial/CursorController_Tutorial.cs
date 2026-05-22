using UnityEngine;

public class CursorController_Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Texture2D cursorTexture;
    
    [Header("Posição do Canhão (Tutorial)")]
    [SerializeField] private Vector2 cannonPosition = new Vector2(-0.12f, -3.36f); // 🔴 POSIÇÃO CORRETA DO TUTORIAL
    
    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 0.5f;
    
    [Header("Cooldown")]
    [SerializeField] private float shootCooldown = 0.5f;
    private float lastShootTime = -1f;

    private GameController_Def myGameController;

    void Start()
    {
        myGameController = FindObjectOfType<GameController_Def>();
        
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        Debug.Log($"CursorController_Tutorial iniciado. Posição do canhão: {cannonPosition}");
        Debug.Log($"MissilePrefab: {(missilePrefab != null ? missilePrefab.name : "NULL")}");
        Debug.Log($"GameController: {(myGameController != null ? "OK" : "NULL")}");
    }

    void Update()
    {
        // 🔴 DEBUG: Mostra o estado a cada 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Estado do GameController: {myGameController?.currentState}");
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"🔴 CLIQUE DETECTADO! Estado: {myGameController?.currentState}, Time: {Time.time}, Cooldown: {lastShootTime + shootCooldown}");
        }
        
        if (myGameController != null && myGameController.currentState != GameController_Def.GameState.Gameplay)
        {
            Debug.Log($"Não atira: estado é {myGameController.currentState}");
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + shootCooldown)
        {
            Debug.Log("✅ CONDIÇÕES OK! Atirando...");
            lastShootTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        if (missilePrefab == null)
        {
            Debug.LogError("❌ MissilePrefab não está atribuído!");
            return;
        }
        
        Vector3 spawnPosition = new Vector3(cannonPosition.x, cannonPosition.y, 0);
        GameObject newMissile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"✅ Míssil tutorial disparado de: {spawnPosition}");
        
        if (audioSource && shootSound)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }
    }
}
using UnityEngine;

public class CursorController_Def : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Texture2D cursorTexture;
    
    [Header("Ponto de disparo (ponta do canhão)")]
    [SerializeField] private Transform firePoint;
    
    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 0.5f;
    
    [Header("Cooldown")]
    [SerializeField] private float shootCooldown = 0.5f;
    private float lastShootTime = -1f;

    private GameController_Def myGameController;
    private GameplayAreaBounds areaBounds;

    void Start()
    {
        myGameController = FindAnyObjectByType<GameController_Def>();
        areaBounds = GameplayAreaBounds.FindInScene();
        
        if (firePoint == null)
            firePoint = CannonFirePoint.Find();
        
        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (missilePrefab == null)
            Debug.LogError("CursorController_Def: MissilePrefab não está atribuído!");
    }

    void Update()
    {
        if (myGameController != null && myGameController.currentState != GameController_Def.GameState.Gameplay)
            return;
        
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        if (missilePrefab == null)
            return;

        if (firePoint == null)
            firePoint = CannonFirePoint.Find();

        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position;
        spawnPosition.z = 0f;

        Vector2 target = CannonFirePoint.GetMouseWorldPosition();

        if (areaBounds != null && !areaBounds.IsWorldPositionInPlayableArea(target))
            return;

        GameObject newMissile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);

        PlayerMissileController_Def missile = newMissile.GetComponent<PlayerMissileController_Def>();
        if (missile != null)
            missile.SetTarget(target);
        
        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound, shootVolume);
    }
}

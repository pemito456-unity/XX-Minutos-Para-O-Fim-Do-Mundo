using UnityEngine;

public class PlayerMissileController_Def : MonoBehaviour
{
    private Vector2 target;
    [SerializeField] private float speed = 8f;
    [SerializeField] private GameObject explosionPrefab;
    
    [Header("Posição do Canhão")]
    [SerializeField] private Vector2 cannonPosition = new Vector2(2.53f, -3.69f);
    
    private bool hasExploded = false;

    void Start()
    {

        transform.position = cannonPosition;
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        Debug.Log($"Míssil spawnado na posição do canhão: {cannonPosition}");
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (hasExploded) return;
        
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            Explode();
        }
    }
    
    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
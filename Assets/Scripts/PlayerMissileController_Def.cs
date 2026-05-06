using UnityEngine;

public class PlayerMissileController_Def : MonoBehaviour
{
    private Vector2 target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private GameObject explosionPrefab;
    
    // 🔴 POSIÇÃO FIXA DO CANHÃO
    private Vector2 cannonPosition = new Vector2(-0.12f, -3.36f);

    void Start()
    {
        // 🔴 Garante que o míssil começa na posição do canhão
        transform.position = cannonPosition;
        
        // Pega a posição do mouse como alvo
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        Debug.Log($"Míssil spawnado na posição do canhão: {cannonPosition}, alvo: {target}");
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public class TutorialMissile : MonoBehaviour
{
    private System.Action onDestroy;
    private System.Action onHitGround;
    private float speed;
    private bool isDestroyed = false;

    [System.Obsolete]
    public void Inicializar(System.Action destroyCallback, System.Action hitGroundCallback, float velocidade)
    {
        onDestroy = destroyCallback;
        onHitGround = hitGroundCallback;
        speed = velocidade;
        
        // Garante que tem Collider
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
        }
        
        // Garante que tem Rigidbody2D para detecção de colisão
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.isKinematic = true;
        }
        
        // Define a tag
        gameObject.tag = "EnemyMissile";
        
        Debug.Log($"Meteoro tutorial criado com tag: {gameObject.tag}");
    }
    
    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        if (transform.position.y < -5f && !isDestroyed)
        {
            DestroyProjectile(false);
        }
    }
    
    void OnMouseDown()
    {
        if (!isDestroyed)
            DestroyProjectile(true);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed)
            return;
        
        if (other.CompareTag("Explosions"))
            DestroyProjectile(true);
    }

    void DestroyProjectile(bool contarComoDestruido)
    {
        if (isDestroyed)
            return;
        isDestroyed = true;

        if (contarComoDestruido && onDestroy != null)
            onDestroy.Invoke();
        else if (!contarComoDestruido && onHitGround != null)
            onHitGround.Invoke();

        ExplosionSpawner.SpawnAt(transform.position);
        Destroy(gameObject);
    }
}
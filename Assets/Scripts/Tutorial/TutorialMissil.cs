using UnityEngine;

public class TutorialMissile : MonoBehaviour
{
    private System.Action onDestroy;
    private System.Action onHitGround;
    private float speed;
    private bool isDestroyed = false;
    
    public void Inicializar(System.Action destroyCallback, System.Action hitGroundCallback, float velocidade)
    {
        onDestroy = destroyCallback;
        onHitGround = hitGroundCallback;
        speed = velocidade;
        
        // Garante que tenha um collider
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
        }
    }
    
    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        if (transform.position.y < -5f && !isDestroyed)
        {
            isDestroyed = true;
            if (onHitGround != null)
                onHitGround.Invoke();
            Destroy(gameObject);
        }
    }
    
    void OnMouseDown()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            if (onDestroy != null)
                onDestroy.Invoke();
            Destroy(gameObject);
        }
    }
}
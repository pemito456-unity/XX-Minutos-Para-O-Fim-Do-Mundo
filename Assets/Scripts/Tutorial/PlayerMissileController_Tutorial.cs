using UnityEngine;

public class PlayerMissileController_Tutorial : MonoBehaviour
{
    private Vector2 target;
    [SerializeField] private float speed = 8f;
    [SerializeField] private GameObject explosionPrefab;
    
    private bool hasExploded = false;
    private GameController_Def gameController;
    private TutorialController2 tutorialController;

    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        tutorialController = Object.FindAnyObjectByType<TutorialController2>();
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target = new Vector2(mousePos.x, mousePos.y);
        
        Debug.Log($"Míssil tutorial spawnado em: {transform.position}, alvo: {target}");
        
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
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 0.5f);
        }
        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("EnemyMissile"))
            {
                // Verifica se é um meteoro do tutorial
                EnemyMissile_Tutorial tutorialMissile = hit.GetComponent<EnemyMissile_Tutorial>();
                if (tutorialMissile != null && tutorialController != null)
                {
                    tutorialController.RegistrarMeteoroDestruido(hit.gameObject);
                }
                else if (gameController != null)
                {
                    gameController.OnMissileIntercepted();
                }
                
                Destroy(hit.gameObject);
            }
        }
        
        Destroy(gameObject);
    }
}
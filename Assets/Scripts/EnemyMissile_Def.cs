using UnityEngine;

public class EnemyMissile_Def : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float speed = 3f;
    private GameObject targetCity;
    private GameController_Def gameController;
    private bool isDestroyed;

    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        FindNewTarget();
    }

    void Update()
    {
        if (isDestroyed)
            return;

        if (targetCity != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetCity.transform.position, speed * Time.deltaTime);
            
            Vector2 direction = (Vector2)targetCity.transform.position - (Vector2)transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        }
        else
        {
            FindNewTarget();
            if (targetCity == null)
                DestroyWithExplosion();
        }
    }

    private void FindNewTarget()
    {
        GameObject[] cities = GameObject.FindGameObjectsWithTag("Defenders");
        if (cities.Length > 0)
            targetCity = cities[Random.Range(0, cities.Length)];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed)
            return;

        if (collision.CompareTag("Explosions"))
        {
            if (gameController != null)
                gameController.OnMissileIntercepted();
            DestroyWithExplosion();
            return;
        }

        if (collision.CompareTag("Defenders"))
        {
            if (gameController != null)
            {
                gameController.OnMissileHitCity();
                gameController.AddRedPressure(15f);
                
                CityScript city = collision.GetComponent<CityScript>();
                if (city != null)
                    city.TakeDamage(1);
            }
            DestroyWithExplosion();
        }
    }

    public void DestroyWithExplosion()
    {
        if (isDestroyed)
            return;
        isDestroyed = true;

        ExplosionSpawner.SpawnAt(transform.position);
        Destroy(gameObject);
    }
}

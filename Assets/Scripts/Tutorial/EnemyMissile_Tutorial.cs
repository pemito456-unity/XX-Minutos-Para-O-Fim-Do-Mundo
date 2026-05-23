using UnityEngine;

public class EnemyMissile_Tutorial : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private GameObject targetCity;
    private TutorialController2 tutorialController;
    private bool isDestroyed;

    void Start()
    {
        tutorialController = Object.FindAnyObjectByType<TutorialController2>();
        FindNewTarget();
        
        if (tutorialController != null)
            tutorialController.RegistrarMeteoroAtivo(gameObject);
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        if (isDestroyed)
            return;

        if (targetCity != null && targetCity.activeInHierarchy)
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
                DestroyWithExplosion(false);
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
            DestroyWithExplosion(true);
            return;
        }

        if (collision.CompareTag("Defenders"))
        {
            DestroyWithExplosion(false);
        }
    }

    public void DestroyWithExplosion(bool contarComoDestruido)
    {
        if (isDestroyed)
            return;
        isDestroyed = true;

        if (tutorialController != null)
        {
            if (contarComoDestruido)
                tutorialController.RegistrarMeteoroDestruido(gameObject);
            else
                tutorialController.RegistrarMeteoroAcertou(gameObject);
        }
        else
            RemoverMeteoroAtivoSeNecessario();

        ExplosionSpawner.SpawnAt(transform.position);
        Destroy(gameObject);
    }

    private void RemoverMeteoroAtivoSeNecessario()
    {
        if (tutorialController != null)
            tutorialController.RemoverMeteoroAtivo(gameObject);
    }

    public void DestroyWithExplosion()
    {
        DestroyWithExplosion(true);
    }
}

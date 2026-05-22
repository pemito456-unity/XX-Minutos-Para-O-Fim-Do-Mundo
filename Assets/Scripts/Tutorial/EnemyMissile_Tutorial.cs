using UnityEngine;

public class EnemyMissile_Tutorial : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private GameObject targetCity;
    private GameController_Def gameController;
    private TutorialController2 tutorialController;

    void Start()
    {
        gameController = Object.FindAnyObjectByType<GameController_Def>();
        tutorialController = Object.FindAnyObjectByType<TutorialController2>();
        FindNewTarget();
        
        // Registra no tutorial
        if (tutorialController != null)
        {
            tutorialController.RegistrarMeteoroAtivo(gameObject);
        }
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
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
            if (targetCity == null) Destroy(gameObject);
        }
    }

    private void FindNewTarget()
    {
        GameObject[] cities = GameObject.FindGameObjectsWithTag("Defenders");
        if (cities.Length > 0)
        {
            targetCity = cities[Random.Range(0, cities.Length)];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Explosions"))
        {
            if (tutorialController != null)
            {
                tutorialController.RegistrarMeteoroDestruido(gameObject);
            }
            Destroy(gameObject);
        }

        if (collision.CompareTag("Defenders"))
        {
            if (tutorialController != null)
            {
                tutorialController.RegistrarMeteoroAcertou(gameObject);
            }
            
            CityScript city = collision.GetComponent<CityScript>();
            if (city != null)
            {
                city.TakeDamage(1);
            }
            Destroy(gameObject);
        }
    }
}
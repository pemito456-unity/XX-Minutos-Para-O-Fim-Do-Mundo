using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private GameObject explosionPrefab;

    private GameObject[] defenders;
    private GameController myGameController;

    private Vector3 target;

    [System.Obsolete]
    void Start()
    {
        Debug.Log("MISSILE NASCEU EM: " + transform.position);
        myGameController = FindObjectOfType<GameController>();

        defenders = GameObject.FindGameObjectsWithTag("Defenders");

        if (defenders.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        target = defenders[Random.Range(0, defenders.Length)].transform.position;
    }

    // recebe velocidade do GameController
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Defenders"))
        {
            Destroy(col.gameObject);

            MissileExplode();

            myGameController.CheckGameOver();
        }
        else if (col.CompareTag("Explosions"))
        {
            MissileExplode();
        }
        else if (col.CompareTag("Ground"))
        {
            MissileExplode();
        }
    }

    private void MissileExplode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
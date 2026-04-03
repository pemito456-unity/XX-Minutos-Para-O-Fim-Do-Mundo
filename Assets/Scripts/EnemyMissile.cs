using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private GameObject explosionPrefab;
    private GameObject[] defenders;
    
    private GameController myGameController;
    
    Vector3 target;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myGameController = GameObject.FindObjectOfType<GameController>();
        defenders = GameObject.FindGameObjectsWithTag("Defenders");
        target = defenders[Random.Range(0, defenders.Length)].transform.position;

        speed = myGameController.enemyMissileSpeed;
        Debug.Log(speed);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Defenders")
        {
            myGameController.EnemyMissileDestroyed();
            MissileExplode();
            Destroy(col.gameObject);
        }
        else if (col.tag == "Explosions")
        {
            //This will add the points for a destroyed enemy missile
            myGameController.AddMissileDestroyedPoints();
            MissileExplode();
        }
    }

    //Spawns Explosions and destroys missile
    private void MissileExplode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    
}
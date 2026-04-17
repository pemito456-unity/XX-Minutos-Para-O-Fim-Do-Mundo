using Unity.VisualScripting;
using UnityEngine;

public class EnemyMissile_Def : MonoBehaviour
{

    [SerializeField] private float speed = 5f;
    [SerializeField] private GameObject explosionPrefab;
    GameObject[] defenders;

    Vector3 target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defenders = GameObject.FindGameObjectsWithTag("Defenders");
        target = defenders[Random.Range(0, defenders.Length)].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Defenders"))
        {
           FindObjectOfType<GameController_Def>().EnemyMissileDestroyed();
           MissileExplode();
           Destroy(col.gameObject);
            
        }
        else if(col.tag == "Explosions")
        {
            FindObjectOfType<GameController_Def>().AddMissileDestroyedPoints(); 
            MissileExplode();
        }
    }

    private void MissileExplode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
           Destroy(gameObject);
    }

}

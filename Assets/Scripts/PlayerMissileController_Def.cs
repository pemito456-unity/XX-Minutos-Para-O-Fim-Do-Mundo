using UnityEngine;



public class PlayerMissileController_Def : MonoBehaviour

{

    private Vector2 target;

    private bool targetSet;

    

    [SerializeField] private float speed = 8f;

    [SerializeField] private GameObject explosionPrefab;

    

    private bool hasExploded;

    private GameController_Def gameController;



    public void SetTarget(Vector2 worldTarget)

    {

        target = worldTarget;

        targetSet = true;

    }



    void Start()

    {

        gameController = Object.FindAnyObjectByType<GameController_Def>();

        

        if (explosionPrefab != null)

            ExplosionSpawner.RegisterPrefab(explosionPrefab);

        

        if (!targetSet)

            target = CannonFirePoint.GetMouseWorldPosition();

        

        Destroy(gameObject, 5f);

    }



    void Update()

    {

        if (hasExploded)

            return;

        

        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        

        if (Vector2.Distance(transform.position, target) < 0.05f)

            Explode();

    }

    

    void Explode()

    {

        if (hasExploded)

            return;

        hasExploded = true;

        

        ExplosionSpawner.SpawnAt(transform.position);

        

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var hit in hitColliders)

        {

            if (!hit.CompareTag("EnemyMissile"))

                continue;



            EnemyMissile_Def enemy = hit.GetComponent<EnemyMissile_Def>();

            if (enemy != null)

                enemy.DestroyWithExplosion();

            else

            {

                ExplosionSpawner.SpawnAt(hit.transform.position);

                Destroy(hit.gameObject);

            }



            if (gameController != null)

                gameController.OnMissileIntercepted();

        }

        

        Destroy(gameObject);

    }

}



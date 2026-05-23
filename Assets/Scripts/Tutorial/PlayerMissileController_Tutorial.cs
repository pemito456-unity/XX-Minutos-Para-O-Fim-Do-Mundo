using System.Collections;
using UnityEngine;

public class PlayerMissileController_Tutorial : MonoBehaviour{
    private Vector2 target;
    private bool targetSet;

    [SerializeField] private float speed = 8f;

    [SerializeField] private GameObject explosionPrefab;

    private bool hasExploded;
    private Coroutine fallbackDestroyCoroutine;

    public void SetTarget(Vector2 worldTarget)
    {
        target = worldTarget;
        targetSet = true;
    }

    void Start()
    {
        if (explosionPrefab != null)
            ExplosionSpawner.RegisterPrefab(explosionPrefab);

        if (!targetSet)
            target = CannonFirePoint.GetMouseWorldPosition();

        fallbackDestroyCoroutine = StartCoroutine(DestroyAfterTimeout(5f));
    }

    IEnumerator DestroyAfterTimeout(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (!hasExploded)
            Destroy(gameObject);
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

        if (fallbackDestroyCoroutine != null)
            StopCoroutine(fallbackDestroyCoroutine);

        ExplosionSpawner.SpawnAt(transform.position);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var hit in hitColliders)
        {
            if (!hit.CompareTag("EnemyMissile"))
                continue;
            EnemyMissile_Tutorial enemy = hit.GetComponent<EnemyMissile_Tutorial>();

            if (enemy != null)
                enemy.DestroyWithExplosion(true);
            else
            {
                ExplosionSpawner.SpawnAt(hit.transform.position);
                Destroy(hit.gameObject);
            }
        }
        Destroy(gameObject);
    }
}



using UnityEngine;

public class EnemyMissileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float Ypadding = 0.5f;

    private float minX, maxX;
    private float yValue;

    void Start()
    {
        Camera cam = Camera.main;

        minX = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;
        maxX = cam.ViewportToWorldPoint(new Vector3(1, 1, 0)).x;
        yValue = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
    }

    // 🔥 ESSENCIAL — usado pelo GameController
    public void SpawnSingleMissile(float speed)
    {
        float randomX = Random.Range(minX, maxX);

        GameObject missile = Instantiate(
            missilePrefab,
            new Vector3(randomX, yValue + Ypadding, 0),
            Quaternion.identity
        );

        // 🔥 passa velocidade pro míssil
        EnemyMissile missileScript = missile.GetComponent<EnemyMissile>();

        if (missileScript != null)
        {
            missileScript.SetSpeed(speed);
        }
    }
}
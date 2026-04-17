using System.Collections;
using UnityEngine;

public class EnemyMissileSpawner_Def : MonoBehaviour
{

    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float Ypadding = 0.5f;

    private float minX, maxX;

    public int missilesToSpawnThisRound = 10;
    public float delayBetweenMissiles = .5f;

    float yValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;
        maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x;

        float randomX = Random.Range(minX, maxX);
        yValue = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;


        //StartCoroutine(SpawnMissiles());
    }

    public void StartRound()
    {
        StartCoroutine(SpawnMissiles());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SpawnMissiles()
    {
        while (missilesToSpawnThisRound > 0)
        {
            float randomX = Random.Range(minX, maxX);

            Instantiate(missilePrefab, new Vector3(randomX, yValue + Ypadding, 0), Quaternion.identity);

            missilesToSpawnThisRound--;

            yield return new WaitForSeconds(delayBetweenMissiles);
        }
    }


}

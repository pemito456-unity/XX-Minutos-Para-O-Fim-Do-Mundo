using System.Collections;
using UnityEngine;

public class EnemyMissileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float Ypadding = 0.5f;

    private float minX, maxX;

    public int MissilesToSpawnThisRound = 10;
    public float DelayBetweenMissiles = .5f;

    private float yValue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;
        maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x;
        
        float randomX = Random.Range(minX, maxX);
        yValue = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        //StartCoroutine(SpawnMissiles());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SpawnMissiles()
    {
        while (MissilesToSpawnThisRound > 0)
        {
            float randomX = Random.Range(minX, maxX);
        
            Instantiate(missilePrefab, new Vector3(randomX, yValue+Ypadding, 0),  Quaternion.identity);
            
            MissilesToSpawnThisRound--;
            
            yield return new WaitForSeconds(DelayBetweenMissiles);
        }
    }
    
    public void StartRound()
    {
        StartCoroutine(SpawnMissiles());
    }
}

using UnityEngine;

public class CityScript : MonoBehaviour
{
    [SerializeField] private int hitsToDestroy = 3;
    private int currentHits;
    private GameController_Def gameController;
    
    void Start()
    {
        currentHits = 0;
        gameController = Object.FindAnyObjectByType<GameController_Def>();
    }
    
    public void TakeDamage(int damage)
    {
        currentHits += damage;
        
        if (currentHits >= hitsToDestroy)
        {
            DestroyBuilding();
        }
        else
        {
            Debug.Log($"{gameObject.name} atingido! {(hitsToDestroy - currentHits)} hits restantes");
            // Opcional: mudar cor ou sprite para mostrar dano
        }
    }
    
    private void DestroyBuilding()
{
    Debug.Log($"{gameObject.name} sendo destruído! Tag: {gameObject.tag}");
    
    if (gameController != null)
    {
        gameController.OnDefenderDestroyed(gameObject);
    }
    else
    {
        Debug.LogError("gameController é NULL no CityScript!");
    }
    
    Destroy(gameObject);
}
}
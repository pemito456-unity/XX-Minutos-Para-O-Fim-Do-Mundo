using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float destroyDelay = 0.5f;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        // Cria o AudioSource via código
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Som 2D
        audioSource.volume = 0.7f;
        
        // Toca o som
        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        else
        {
            Debug.LogWarning("ExplosionSound não atribuído!");
        }
        
        // Destroi o objeto após a animação
        Destroy(gameObject, destroyDelay);
    }
}
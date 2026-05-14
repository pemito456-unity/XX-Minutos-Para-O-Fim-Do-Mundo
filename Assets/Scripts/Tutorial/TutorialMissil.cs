using UnityEngine;

public class TutorialMissile : MonoBehaviour
{
    private TutorialController controller;
    private float speed;
    private bool isDestroyed = false;
    
    public void Inicializar(TutorialController tutorialController, float velocidade)
    {
        controller = tutorialController;
        speed = velocidade;
    }
    
    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        if (transform.position.y < -5f && !isDestroyed)
        {
            isDestroyed = true;
            if (controller != null)
                controller.RegistrarMeteoroAcertou(gameObject);
            Destroy(gameObject);
        }
    }
    
    void OnMouseDown()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            if (controller != null)
                controller.RegistrarMeteoroDestruido(gameObject);
            
            Destroy(gameObject);
        }
    }
}
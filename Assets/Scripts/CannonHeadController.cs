using UnityEngine;

public class CannonHeadController : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool smoothRotation = true;
    
    [Header("Limites (opcional)")]
    [SerializeField] private float minAngle = -60f;
    [SerializeField] private float maxAngle = 60f;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        RotateToMouse();
    }
    
    void RotateToMouse()
    {
        // Pega a posição do mouse no mundo
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Calcula direção do canhão para o mouse
        Vector2 direction = mousePos - transform.position;
        
        // Calcula o ângulo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 🔴 AJUSTE: Se o canhão apontar para CIMA, use angle
        // Se apontar para DIREITA, use angle - 90
        // Teste qual funciona melhor
        angle -= 90f;
        
        // Aplica limites
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        
        // Aplica rotação
        if (smoothRotation)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PhoneButton : MonoBehaviour
{
    private DialogueUITest dialogueManager;
    private Button button;
    
    void Start()
    {
        // Procura o DialogueUITest na cena
        dialogueManager = Object.FindAnyObjectByType<DialogueUITest>();
        button = GetComponent<Button>();
        
        if (button != null)
            button.onClick.AddListener(OnPhoneClicked);
        
        // Começa desativado
        gameObject.SetActive(false);
        
        Debug.Log($"PhoneButton inicializado. DialogueUITest encontrado: {dialogueManager != null}");
    }
    
    void OnPhoneClicked()
    {
        Debug.Log("❗ PHONE BUTTON CLICADO!");
        
        if (dialogueManager != null)
        {
            Debug.Log("Chamando AnswerPhone()");
            dialogueManager.AnswerPhone();
        }
        else
        {
            Debug.LogError("❗ dialogueManager é NULL! Verifique se o DialogueUITest está na cena.");
        }
    }
}
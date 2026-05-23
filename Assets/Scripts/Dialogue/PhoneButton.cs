using UnityEngine;
using UnityEngine.UI;

public class PhoneButton : MonoBehaviour
{
    private DialogueUITest dialogueManager;
    private Button button;

    void Awake()
    {
        dialogueManager = Object.FindAnyObjectByType<DialogueUITest>();
        button = GetComponent<Button>();

        if (button != null)
        {
            DialogueButtonStyling.ApplyChoiceButtonHover(button);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPhoneClicked);
        }
    }

    void OnPhoneClicked()
    {
        if (dialogueManager != null)
            dialogueManager.AnswerPhone();
    }
}

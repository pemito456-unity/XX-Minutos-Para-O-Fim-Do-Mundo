using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private Button optionA;
    [SerializeField] private Button optionB;
    [SerializeField] private Button optionC;

    private GameController gameController;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    public void StartDialogue()
    {
        gameController.currentState = GameController.GameState.Dialogue;

        dialoguePanel.SetActive(true);

        dialogueText.text = "(Telefone) \"General Alastair, o que está acontecendo? De onde estão vindo estes mísseis?\"";

        optionA.gameObject.SetActive(true);
        optionB.gameObject.SetActive(true);
        optionC.gameObject.SetActive(true);
    }

    public void EscolhaA()
    {
        dialogueText.text = "(Telefone) General: Isso é de se esperar de mísseis intercontinentais. Confirme o que está acontecendo imediatamente.";

        HideButtons();

        StartCoroutine(WaitAndFinish());
    }

    public void EscolhaB()
    {
        dialogueText.text = "(Telefone) General: Do norte? União Soviética! DEFCON-2 ativado.";

        HideButtons();

        gameController.enemyMissileSpeed *= 1.3f;

        StartCoroutine(WaitAndFinish());
    }

    public void EscolhaC()
    {
        dialogueText.text = "(Telefone) General: Impossível! Não deixe nenhum objeto atingir o solo!";

        HideButtons();

        gameController.enemyMissileSpeed *= 0.7f;

        StartCoroutine(WaitAndFinish());
    }

    void HideButtons()
    {
        optionA.gameObject.SetActive(false);
        optionB.gameObject.SetActive(false);
        optionC.gameObject.SetActive(false);
    }

    IEnumerator WaitAndFinish()
    {
        yield return new WaitForSecondsRealtime(2f);
        FinishDialogue();
    }

    void FinishDialogue()
    {
        dialoguePanel.SetActive(false);
        gameController.ContinueGameFromDialogue();
        
    }
}
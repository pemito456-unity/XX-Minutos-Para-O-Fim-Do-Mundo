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

    private bool isDialogueActive = false;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    public void StartDialogue()
    {
        if (isDialogueActive) return;

        isDialogueActive = true;

        // pausa o jogo
        Time.timeScale = 0f;

        dialoguePanel.SetActive(true);

        dialogueText.text = "(Telefone) \"General Alastair, o que está acontecendo? De onde estão vindo estes mísseis?\"";

        optionA.gameObject.SetActive(true);
        optionB.gameObject.SetActive(true);
        optionC.gameObject.SetActive(true);
    }

    public void EscolhaA()
    {
        dialogueText.text = "(Telefone) General: Isso é de se esperar de mísseis intercontinentais. Confirme imediatamente.";

        HideButtons();

        // mantém tensão (não muda nada)

        StartCoroutine(WaitAndFinish());
    }

    public void EscolhaB()
    {
        dialogueText.text = "(Telefone) General: Do norte? União Soviética! DEFCON-2 ativado.";

        HideButtons();

        // tensão aumenta
        gameController.enemyMissileSpeed *= 1.3f;

        StartCoroutine(WaitAndFinish());
    }

    public void EscolhaC()
    {
        dialogueText.text = "(Telefone) General: Impossível! Não deixe nenhum objeto atingir o solo!";

        HideButtons();

        // tensão diminui
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

        // volta o jogo
        Time.timeScale = 1f;

        isDialogueActive = false;
    }
}
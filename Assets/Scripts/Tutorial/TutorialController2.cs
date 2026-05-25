using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TutorialController2 : MonoBehaviour
{
    [Header("Configurações do Tutorial")]
    [SerializeField] private int meteorosNecessarios = 10;
    [SerializeField] private float tempoEntreMeteoros = 1.5f;
    [SerializeField] private float velocidadeMeteoros = 2f;
    
    [Header("Referências")]
    [SerializeField] private EnemyMissileSpawner_Tutorial spawnerTutorial;
    [SerializeField] private GameController_Def gameController;
    [SerializeField] private DialogueUITutorial dialogueManager;
    
    [Header("Diálogos do Tutorial (pré-prontos)")]
    [SerializeField] private DialogueData dialogoIntroducao;
    [SerializeField] private DialogueData dialogoPressao;
    [SerializeField] private DialogueData dialogoTransicaoCena;
    
    [Header("Próxima Cena")]
    [SerializeField] private string proximaCena = "Principal_Missil";
    
    private int meteorosDestruidos;
    private List<GameObject> meteorosAtivos = new List<GameObject>();
    private bool ondaEmAndamento;
    private Coroutine tutorialCoroutine;

    void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueUITutorial>();

        if (spawnerTutorial == null)
            spawnerTutorial = FindAnyObjectByType<EnemyMissileSpawner_Tutorial>();

        if (spawnerTutorial == null)
            Debug.LogError("TutorialController2: EnemyMissileSpawner_Tutorial não encontrado na cena!");

        spawnerTutorial?.StopSpawning();
        tutorialCoroutine = StartCoroutine(RunTutorial());
    }

    public void SkipTutorial()
    {
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }

        StopAllCoroutines();
        ondaEmAndamento = false;
        spawnerTutorial?.StopSpawning();

        meteorosAtivos.RemoveAll(m => m == null);
        foreach (GameObject meteoro in meteorosAtivos)
        {
            if (meteoro != null)
                Destroy(meteoro);
        }
        meteorosAtivos.Clear();

        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;

        Debug.Log("Tutorial pulado — carregando cena principal.");
        IniciarCenaPrincipal();
    }
    
    IEnumerator RunTutorial()
    {
        yield return StartCoroutine(MostrarDialogo(dialogoIntroducao));
        yield return StartCoroutine(ExplicarMeteoros());
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Gameplay;
        Time.timeScale = 1f;
        
        yield return StartCoroutine(OndaDeMeteoros());
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Dialogue;
        Time.timeScale = 0f;
        
        yield return StartCoroutine(MostrarDialogo(dialogoPressao));
        yield return StartCoroutine(MostrarDialogo(dialogoTransicaoCena));
        
        IniciarCenaPrincipal();
    }
    
    IEnumerator MostrarDialogo(DialogueData dialogo)
    {
        if (dialogo == null || dialogueManager == null)
            yield break;
        
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Dialogue;
        Time.timeScale = 0f;
        
        bool dialogoCompleto = false;
        dialogueManager.ShowDialogue(dialogo, () => dialogoCompleto = true);
        
        while (!dialogoCompleto)
            yield return null;
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    IEnumerator ExplicarMeteoros()
    {
        if (dialogueManager == null)
            yield break;
        
        DialogueData explicacao = ScriptableObject.CreateInstance<DialogueData>();
        explicacao.speakerName = "Coronel Adams";
        explicacao.dialogueText = $"Destrua {meteorosNecessarios} projéteis para continuar o treinamento.\n\nUse o canhão clicando na direção do alvo!";
        
        List<DialogueChoice> choices = new List<DialogueChoice>();
        DialogueChoice choice = new DialogueChoice();
        choice.buttonText = "Entendi. Estou pronto!";
        choice.speakerReaction = "Ótimo! Os projéteis já estão a caminho.";
        choices.Add(choice);
        explicacao.choices = choices;
        
        bool completo = false;
        dialogueManager.ShowDialogue(explicacao, () => completo = true);
        
        while (!completo)
            yield return null;
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    IEnumerator OndaDeMeteoros()
    {
        meteorosDestruidos = 0;
        meteorosAtivos.Clear();
        ondaEmAndamento = true;
        
        if (spawnerTutorial == null)
        {
            Debug.LogError("TutorialController2: não foi possível iniciar o spawn — spawner ausente.");
            yield break;
        }
        
        spawnerTutorial.delayBetweenMissiles = tempoEntreMeteoros;
        spawnerTutorial.SetMissileSpeed(velocidadeMeteoros);
        spawnerTutorial.StartSpawning();
        
        while (meteorosDestruidos < meteorosNecessarios)
            yield return null;
        
        ondaEmAndamento = false;
        spawnerTutorial.StopSpawning();
        
        meteorosAtivos.RemoveAll(m => m == null);
        foreach (GameObject m in meteorosAtivos)
        {
            if (m != null)
                Destroy(m);
        }
        meteorosAtivos.Clear();
        
        yield return new WaitForSeconds(0.5f);
    }
    
    public void RegistrarMeteoroAtivo(GameObject meteoro)
    {
        if (ondaEmAndamento && !meteorosAtivos.Contains(meteoro))
            meteorosAtivos.Add(meteoro);
    }
    
    public void RemoverMeteoroAtivo(GameObject meteoro)
    {
        meteorosAtivos.Remove(meteoro);
    }
    
    public void RegistrarMeteoroDestruido(GameObject meteoro)
    {
        if (!ondaEmAndamento)
            return;
        
        meteorosDestruidos++;
        RemoverMeteoroAtivo(meteoro);
    }
    
    public void RegistrarMeteoroAcertou(GameObject meteoro)
    {
        RemoverMeteoroAtivo(meteoro);
    }
    
    void IniciarCenaPrincipal()
    {
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(proximaCena))
            SceneManager.LoadScene(proximaCena);
        else
            Debug.LogError("TutorialController2: nome da cena principal não configurado!");
    }
}

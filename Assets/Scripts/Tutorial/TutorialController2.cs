using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class TutorialController2 : MonoBehaviour
{
    [Header("Configurações do Tutorial")]
    [SerializeField] private int meteorosNaOnda = 5;
    [SerializeField] private float tempoEntreMeteoros = 1.5f;
    [SerializeField] private float velocidadeMeteoros = 2f;
    
    [Header("Referências")]
    [SerializeField] private EnemyMissileSpawner_Tutorial spawnerTutorial;
    [SerializeField] private GameController_Def gameController;
    [SerializeField] private DialogueUITutorial dialogueManager;
    
    [Header("Diálogos do Tutorial")]
    [SerializeField] private DialogueData dialogoIntroducao;
    
    private int meteorosDestruidos = 0;
    private int meteorosAcertaram = 0;
    private List<GameObject> meteorosAtivos = new List<GameObject>();

    [System.Obsolete]
    void Start()
    {
        Debug.Log("=== TUTORIAL INICIADO ===");
        
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueUITutorial>();
        }
        
        if (spawnerTutorial != null)
        {
            spawnerTutorial.StopSpawning();
        }
        
        StartCoroutine(RunTutorial());
    }
    
    IEnumerator RunTutorial()
    {
        // Durante os diálogos, o estado é Dialogue (não atira)
        yield return StartCoroutine(MostrarDialogo(dialogoIntroducao));
        yield return StartCoroutine(ExplicarMeteoros());
        
        // 🔴 MUDA PARA GAMEPLAY DURANTE A ONDA
        if (gameController != null)
        {
            gameController.currentState = GameController_Def.GameState.Gameplay;
            Debug.Log("Estado alterado para GAMEPLAY - pode atirar!");
        }
        Time.timeScale = 1f;
        
        yield return StartCoroutine(OndaDeMeteoros());
        
        // Volta para Dialogue para o resultado
        if (gameController != null)
        {
            gameController.currentState = GameController_Def.GameState.Dialogue;
        }
        Time.timeScale = 0f;
        
        yield return StartCoroutine(ResultadoOnda());
        
        FinalizarTutorial();
    }
    
    IEnumerator MostrarDialogo(DialogueData dialogo)
    {
        if (dialogo == null) yield break;
        if (dialogueManager == null) yield break;
        
        // Durante diálogo, estado Dialogue
        if (gameController != null)
            gameController.currentState = GameController_Def.GameState.Dialogue;
        Time.timeScale = 0f;
        
        bool dialogoCompleto = false;
        
        dialogueManager.ShowDialogue(dialogo, () => {
            dialogoCompleto = true;
        });
        
        while (!dialogoCompleto)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    IEnumerator ExplicarMeteoros()
    {
        if (dialogueManager == null) yield break;
        
        DialogueData explicacao = ScriptableObject.CreateInstance<DialogueData>();
        explicacao.speakerName = "Coronel Adams";
        explicacao.dialogueText = "Clique nos meteoros para destruí-los!\n\nProteja Washington dos impactos.";
        
        List<DialogueChoice> choices = new List<DialogueChoice>();
        DialogueChoice choice = new DialogueChoice();
        choice.buttonText = "Entendi. Estou pronto!";
        choice.speakerReaction = "Ótimo! Vamos começar.";
        choices.Add(choice);
        explicacao.choices = choices;
        
        bool completo = false;
        
        dialogueManager.ShowDialogue(explicacao, () => {
            completo = true;
        });
        
        while (!completo)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    IEnumerator OndaDeMeteoros()
    {
        Debug.Log($"Iniciando onda de {meteorosNaOnda} meteoros...");
        
        meteorosDestruidos = 0;
        meteorosAcertaram = 0;
        meteorosAtivos.Clear();
        
        if (spawnerTutorial != null)
        {
            spawnerTutorial.delayBetweenMissiles = tempoEntreMeteoros;
            spawnerTutorial.StartSpawning();
        }
        
        // Aguarda todos os meteoros serem spawnados
        yield return new WaitForSeconds(meteorosNaOnda * tempoEntreMeteoros + 2f);
        
        // Aguarda os meteoros ativos acabarem
        float timeout = 15f;
        float elapsed = 0f;
        
        while (meteorosAtivos.Count > 0 && elapsed < timeout)
        {
            meteorosAtivos.RemoveAll(m => m == null);
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        
        if (spawnerTutorial != null)
        {
            spawnerTutorial.StopSpawning();
        }
        
        // Destroi meteoros restantes
        foreach (GameObject m in meteorosAtivos)
        {
            if (m != null) Destroy(m);
        }
        meteorosAtivos.Clear();
        
        Debug.Log($"Onda finalizada. Destruídos: {meteorosDestruidos}, Acertaram: {meteorosAcertaram}");
        yield return new WaitForSeconds(1f);
    }
    
    public void RegistrarMeteoroAtivo(GameObject meteoro)
    {
        if (!meteorosAtivos.Contains(meteoro))
        {
            meteorosAtivos.Add(meteoro);
        }
    }
    
    public void RemoverMeteoroAtivo(GameObject meteoro)
    {
        if (meteorosAtivos.Contains(meteoro))
        {
            meteorosAtivos.Remove(meteoro);
        }
    }
    
    public void RegistrarMeteoroDestruido(GameObject meteoro)
    {
        meteorosDestruidos++;
        RemoverMeteoroAtivo(meteoro);
        Debug.Log($"💥 Meteoro destruído! ({meteorosDestruidos}/{meteorosNaOnda})");
    }
    
    public void RegistrarMeteoroAcertou(GameObject meteoro)
    {
        meteorosAcertaram++;
        RemoverMeteoroAtivo(meteoro);
        Debug.Log($"💔 Meteoro acertou o chão! ({meteorosAcertaram}/{meteorosNaOnda})");
    }
    
    IEnumerator ResultadoOnda()
    {
        if (dialogueManager == null) yield break;
        
        DialogueData resultado = ScriptableObject.CreateInstance<DialogueData>();
        resultado.speakerName = "Coronel Adams";
        
        if (meteorosDestruidos >= meteorosNaOnda - 2)
        {
            resultado.dialogueText = $"Ótimo! Você destruiu {meteorosDestruidos} de {meteorosNaOnda} meteoros!\n\nContinue assim!";
        }
        else if (meteorosDestruidos >= meteorosNaOnda / 2)
        {
            resultado.dialogueText = $"Bom trabalho! Você destruiu {meteorosDestruidos} de {meteorosNaOnda} meteoros.\n\nTente acertar mais na próxima!";
        }
        else
        {
            resultado.dialogueText = $"Você destruiu {meteorosDestruidos} de {meteorosNaOnda} meteoros.\n\nLembre-se: clique nos meteoros para destruí-los!";
        }
        
        List<DialogueChoice> choices = new List<DialogueChoice>();
        DialogueChoice choice = new DialogueChoice();
        choice.buttonText = "Continuar";
        choice.speakerReaction = "Bom trabalho! Tutorial concluído.";
        choices.Add(choice);
        resultado.choices = choices;
        
        bool completo = false;
        
        dialogueManager.ShowDialogue(resultado, () => {
            completo = true;
        });
        
        while (!completo)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    void FinalizarTutorial()
    {
        Debug.Log("TUTORIAL FINALIZADO!");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Configuração de Cenas")]
    [Tooltip("Digite o nome da cena que deve carregar")]
    public string proximaCena;

    [Header("Painéis do Menu")]
    public GameObject painelPrincipal;
    public GameObject painelCreditos;
    public GameObject livroCreditos; // 🔴 Adicione a referência do livro separadamente

    [Header("Botões (opcional - para referência)")]
    public Button botaoJogar;
    public Button botaoCreditos;
    public Button botaoSair;
    public Button botaoVoltarCreditos;

    void Start()
    {
        MostrarPainelPrincipal();
        ConfigurarBotoes();
    }

    void ConfigurarBotoes()
    {
        // Configura botão de créditos
        if (botaoCreditos != null)
        {
            botaoCreditos.onClick.RemoveAllListeners();
            botaoCreditos.onClick.AddListener(MostrarPainelCreditos);
        }
        
        // Configura botão voltar dos créditos
        if (botaoVoltarCreditos != null)
        {
            botaoVoltarCreditos.onClick.RemoveAllListeners();
            botaoVoltarCreditos.onClick.AddListener(MostrarPainelPrincipal);
            Debug.Log("Botão Voltar configurado!");
        }
        else
        {
            // Tenta encontrar o botão dentro do painel
            if (painelCreditos != null)
            {
                Button voltarBtn = painelCreditos.GetComponentInChildren<Button>();
                if (voltarBtn != null)
                {
                    voltarBtn.onClick.RemoveAllListeners();
                    voltarBtn.onClick.AddListener(MostrarPainelPrincipal);
                    Debug.Log("Botão Voltar encontrado automaticamente!");
                }
            }
        }
        
        // Configura botão jogar
        if (botaoJogar != null)
        {
            botaoJogar.onClick.RemoveAllListeners();
            botaoJogar.onClick.AddListener(MostrarPainelJogar);
        }
        
        // Configura botão sair
        if (botaoSair != null)
        {
            botaoSair.onClick.RemoveAllListeners();
            botaoSair.onClick.AddListener(SairDoJogo);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (painelCreditos != null && painelCreditos.activeSelf)
            {
                MostrarPainelPrincipal();
                Debug.Log("ESC pressionado: voltando ao menu principal");
            }
        }
    }

    public void MostrarPainelPrincipal()
    {
        DesativarTodosOsPaineis();
        
        if (painelPrincipal != null) 
        {
            painelPrincipal.SetActive(true);
            Debug.Log("Painel principal ativado");
        }
    }

    public void MostrarPainelJogar()
    {
        if (!string.IsNullOrEmpty(proximaCena))
        {
            Debug.Log($"Carregando cena: {proximaCena}");
            SceneManager.LoadScene(proximaCena);
        }
        else
        {
            Debug.LogError("O nome da próxima cena não foi definido no Inspector!");
        }
    }

    public void MostrarPainelCreditos()
    {
        Debug.Log("=== MOSTRANDO CRÉDITOS ===");
        
        // Primeiro, desativa tudo
        if (painelPrincipal != null) painelPrincipal.SetActive(false);
        
        // Ativa o painel de créditos
        if (painelCreditos != null)
        {
            painelCreditos.SetActive(true);
            Debug.Log($"Painel de créditos ativado. ActiveSelf: {painelCreditos.activeSelf}");
        }
        
        // 🔴 GARANTE QUE O LIVRO ESTÁ ATIVO
        if (livroCreditos != null)
        {
            livroCreditos.SetActive(true);
            Debug.Log($"Livro de créditos ativado. ActiveSelf: {livroCreditos.activeSelf}");
        }
        else
        {
            // Se não tem referência separada, tenta encontrar
            if (painelCreditos != null)
            {
                Transform livro = painelCreditos.transform.Find("Livro Creditos");
                if (livro != null)
                {
                    livro.gameObject.SetActive(true);
                    Debug.Log("Livro de créditos encontrado e ativado!");
                }
                else
                {
                    Debug.LogWarning("'Livro Creditos' não encontrado como filho do painelCreditos!");
                }
            }
        }
    }

    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    private void DesativarTodosOsPaineis()
    {
        if (painelPrincipal != null) painelPrincipal.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }
}
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

    [Header("Botões (opcional - para referência)")]
    public Button botaoJogar;
    public Button botaoCreditos;
    public Button botaoSair;
    public Button botaoVoltarCreditos;

    void Start()
    {
        MostrarPainelPrincipal();
        
        if (botaoVoltarCreditos != null)
        {
            botaoVoltarCreditos.onClick.RemoveAllListeners();
            botaoVoltarCreditos.onClick.AddListener(MostrarPainelPrincipal);
            Debug.Log("Botão Voltar configurado!");
        }
        else
        {
            if (painelCreditos != null)
            {
                Button voltarBtn = painelCreditos.GetComponentInChildren<Button>();
                if (voltarBtn != null)
                {
                    voltarBtn.onClick.RemoveAllListeners();
                    voltarBtn.onClick.AddListener(MostrarPainelPrincipal);
                    Debug.Log("Botão Voltar encontrado automaticamente!");
                }
                else
                {
                    Debug.LogWarning("Nenhum botão 'Voltar' encontrado dentro do painelCreditos!");
                }
            }
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
        DesativarTodosOsPaineis();
        if (painelCreditos != null) 
        {
            painelCreditos.SetActive(true);
            Debug.Log("Painel de créditos ativado");
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
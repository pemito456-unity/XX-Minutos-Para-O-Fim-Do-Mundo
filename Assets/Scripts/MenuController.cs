using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Configuração de Cenas")]
    [Tooltip("Digite o nome da cena que deve carregar")]
    public string proximaCena;

    [Header("Painéis do Menu")]
    public GameObject painelPrincipal;
    public GameObject painelCreditos;

    void Start()
    {
        MostrarPainelPrincipal();
    }

    public void MostrarPainelPrincipal()
    {
        DesativarTodosOsPaineis();
        if (painelPrincipal) painelPrincipal.SetActive(true);
    }

    public void MostrarPainelJogar()
    {
        if (!string.IsNullOrEmpty(proximaCena))
        {
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
        if (painelCreditos) painelCreditos.SetActive(true);
    }

    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    private void DesativarTodosOsPaineis()
    {
        if (painelPrincipal) painelPrincipal.SetActive(false);
        if (painelCreditos) painelCreditos.SetActive(false);
    }
}
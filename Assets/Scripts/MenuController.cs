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
    [Tooltip("Raiz do painel de créditos (objeto Credits na cena).")]
    public GameObject painelCreditos;
    [Tooltip("Imagem de fundo do painel (filho PainelCreditos). Arraste sua arte aqui depois.")]
    [SerializeField] private Image imagemPainelCreditos;

    [Header("Botões (opcional - encontrados por nome se vazios)")]
    public Button botaoJogar;
    public Button botaoCreditos;
    public Button botaoSair;
    public Button botaoVoltarCreditos;

    [Header("Áudio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField] [Range(0f, 1f)] private float backgroundMusicVolume = 0.5f;
    [SerializeField] private AudioSource buttonClickAudioSource;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] [Range(0f, 1f)] private float buttonClickVolume = 1f;

    void Start()
    {
        GarantirAudioSources();
        ResolverReferencias();
        MostrarPainelPrincipal();
        ConfigurarBotoes();
        PlayBackgroundMusic();
    }

    void OnDestroy()
    {
        StopBackgroundMusic();
    }

    void ResolverReferencias()
    {
        if (botaoCreditos == null)
            botaoCreditos = EncontrarBotao("ButtonCredits");
        if (botaoVoltarCreditos == null)
            botaoVoltarCreditos = EncontrarBotao("ButtonBack");
        if (botaoJogar == null)
            botaoJogar = EncontrarBotao("ButtonPlay");
        if (botaoSair == null)
            botaoSair = EncontrarBotao("ButtonExit");

        if (imagemPainelCreditos == null && painelCreditos != null)
        {
            Transform painel = painelCreditos.transform.Find("PainelCreditos");
            if (painel != null)
                imagemPainelCreditos = painel.GetComponent<Image>();
        }
    }

    static Button EncontrarBotao(string nome)
    {
        GameObject go = GameObject.Find(nome);
        return go != null ? go.GetComponent<Button>() : null;
    }

    void ConfigurarBotoes()
    {
        ConfigurarBotao(botaoCreditos, MostrarPainelCreditos);
        ConfigurarBotao(botaoVoltarCreditos, MostrarPainelPrincipal);
        ConfigurarBotao(botaoJogar, MostrarPainelJogar);
        ConfigurarBotao(botaoSair, SairDoJogo);
    }

    void ConfigurarBotao(Button botao, UnityEngine.Events.UnityAction acao)
    {
        if (botao == null || acao == null)
            return;

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() =>
        {
            PlayButtonClickSound();
            acao();
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && painelCreditos != null && painelCreditos.activeSelf)
            MostrarPainelPrincipal();
    }

    public void MostrarPainelPrincipal()
    {
        if (painelCreditos != null)
            painelCreditos.SetActive(false);

        if (painelPrincipal != null)
            painelPrincipal.SetActive(true);
    }

    public void MostrarPainelJogar()
    {
        if (!string.IsNullOrEmpty(proximaCena))
            SceneManager.LoadScene(proximaCena);
        else
            Debug.LogError("O nome da próxima cena não foi definido no Inspector!");
    }

    public void MostrarPainelCreditos()
    {
        if (painelPrincipal != null)
            painelPrincipal.SetActive(false);

        if (painelCreditos != null)
            painelCreditos.SetActive(true);
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }

    void GarantirAudioSources()
    {
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource.playOnAwake = false;
            musicAudioSource.loop = true;
        }

        if (buttonClickAudioSource == null)
        {
            foreach (AudioSource source in GetComponents<AudioSource>())
            {
                if (source != musicAudioSource)
                {
                    buttonClickAudioSource = source;
                    break;
                }
            }

            if (buttonClickAudioSource == null)
                buttonClickAudioSource = gameObject.AddComponent<AudioSource>();

            buttonClickAudioSource.playOnAwake = false;
            buttonClickAudioSource.loop = false;
        }
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicClip == null || musicAudioSource == null)
            return;

        musicAudioSource.clip = backgroundMusicClip;
        musicAudioSource.volume = backgroundMusicVolume;
        musicAudioSource.loop = true;

        if (!musicAudioSource.isPlaying)
            musicAudioSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
            musicAudioSource.Stop();
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip == null || buttonClickAudioSource == null)
            return;

        buttonClickAudioSource.PlayOneShot(buttonClickClip, buttonClickVolume);
    }
}

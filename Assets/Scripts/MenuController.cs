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

    [Header("Botões (opcional - encontrados por nome se vazios)")]
    public Button botaoJogar;
    public Button botaoCreditos;
    public Button botaoSair;
    public Button botaoVoltarCreditos;

    [Header("Áudio")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField] [Range(0f, 1f)] private float backgroundMusicVolume = 0.5f;
    [SerializeField] private AudioSource responseFeedbackAudioSource;
    [SerializeField] private AudioClip positiveResponseClip;
    [SerializeField] private AudioClip negativeResponseClip;
    [SerializeField] [Range(0f, 1f)] private float responseFeedbackVolume = 1f;

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
    }

    static Button EncontrarBotao(string nome)
    {
        GameObject go = GameObject.Find(nome);
        return go != null ? go.GetComponent<Button>() : null;
    }

    void ConfigurarBotoes()
    {
        if (botaoCreditos != null)
        {
            botaoCreditos.onClick.RemoveAllListeners();
            botaoCreditos.onClick.AddListener(MostrarPainelCreditos);
        }

        if (botaoVoltarCreditos != null)
        {
            botaoVoltarCreditos.onClick.RemoveAllListeners();
            botaoVoltarCreditos.onClick.AddListener(MostrarPainelPrincipal);
        }

        if (botaoJogar != null)
        {
            botaoJogar.onClick.RemoveAllListeners();
            botaoJogar.onClick.AddListener(MostrarPainelJogar);
        }

        if (botaoSair != null)
        {
            botaoSair.onClick.RemoveAllListeners();
            botaoSair.onClick.AddListener(SairDoJogo);
        }
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

        if (responseFeedbackAudioSource == null)
        {
            foreach (AudioSource source in GetComponents<AudioSource>())
            {
                if (source != musicAudioSource)
                {
                    responseFeedbackAudioSource = source;
                    break;
                }
            }

            if (responseFeedbackAudioSource == null)
                responseFeedbackAudioSource = gameObject.AddComponent<AudioSource>();

            responseFeedbackAudioSource.playOnAwake = false;
            responseFeedbackAudioSource.loop = false;
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

    public void PlayPositiveResponseFeedbackSound()
    {
        if (positiveResponseClip == null || responseFeedbackAudioSource == null)
            return;

        responseFeedbackAudioSource.PlayOneShot(positiveResponseClip, responseFeedbackVolume);
    }

    public void PlayNegativeResponseFeedbackSound()
    {
        if (negativeResponseClip == null || responseFeedbackAudioSource == null)
            return;

        responseFeedbackAudioSource.PlayOneShot(negativeResponseClip, responseFeedbackVolume);
    }
}

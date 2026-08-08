using UnityEngine;

public class SkillCheckManager : MonoBehaviour
{
    public static SkillCheckManager Instance;

    [Header("UI Elementos (Barra Horizontal)")]
    public GameObject painelSkillCheck;
    public RectTransform barraFundo;
    public RectTransform zonaDeAcerto;
    public RectTransform ponteiro;

    [Header("Configurações")]
    public float velocidade = 600f;
    public int peixesPerdidosAoFalhar = 2;

    private bool movendoParaDireita = true;
    private bool jogoAtivo = false;
    private int errosCometidos = 0;

    private float limiteEsquerda;
    private float limiteDireita;

    [Header("Efeitos Sonoros & Música")]
    public AudioSource musicaPrincipal;
    public AudioSource audioSourceSFX;

    [Space(10)]
    public AudioClip musicaTensa;
    public AudioClip somAcerto;
    public AudioClip somErro;
    public AudioClip somVomito;

    [Header("Gerenciadores")]
    public IconManager iconManager;
    public GameController gameController;
    public FishRespawnManager respawnManager;

    [Header("Modo de Cena")]
    public bool modoAnaliseLaboratorio = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        AtualizarLimites();
    }

    void AtualizarLimites()
    {
        if (barraFundo != null)
        {
            float larguraBarra = barraFundo.rect.width;
            limiteEsquerda = -larguraBarra / 2f;
            limiteDireita = larguraBarra / 2f;
        }
    }

    void Update()
    {
        if (!jogoAtivo) return;

        float deslocamento = velocidade * Time.deltaTime;

        if (movendoParaDireita)
        {
            ponteiro.anchoredPosition += new Vector2(deslocamento, 0);
            if (ponteiro.anchoredPosition.x >= limiteDireita)
                movendoParaDireita = false;
        }
        else
        {
            ponteiro.anchoredPosition -= new Vector2(deslocamento, 0);
            if (ponteiro.anchoredPosition.x <= limiteEsquerda)
                movendoParaDireita = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ValidarClique();
        }
    }

    public void IniciarSequenciaSkillCheck()
    {
        errosCometidos = 0;

        if (musicaPrincipal != null && musicaPrincipal.isPlaying)
        {
            musicaPrincipal.Pause();
        }

        if (audioSourceSFX != null && musicaTensa != null)
        {
            audioSourceSFX.clip = musicaTensa;
            audioSourceSFX.loop = true;
            audioSourceSFX.Play();
        }

        ProximoSkillCheck();
    }

    void ProximoSkillCheck()
    {
        AtualizarLimites();
        if (painelSkillCheck != null) painelSkillCheck.SetActive(true);
        jogoAtivo = true;

        if (ponteiro != null)
            ponteiro.anchoredPosition = new Vector2(limiteEsquerda, ponteiro.anchoredPosition.y);

        movendoParaDireita = true;

        if (zonaDeAcerto != null)
        {
            float metadeZona = zonaDeAcerto.rect.width / 2f;
            float xAleatorio = Random.Range(limiteEsquerda + metadeZona, limiteDireita - metadeZona);
            zonaDeAcerto.anchoredPosition = new Vector2(xAleatorio, zonaDeAcerto.anchoredPosition.y);
        }
    }

    void ValidarClique()
    {
        jogoAtivo = false;

        float posPonteiroX = ponteiro.anchoredPosition.x;
        float zonaInicioX = zonaDeAcerto.anchoredPosition.x - (zonaDeAcerto.rect.width / 2f);
        float zonaFimX = zonaDeAcerto.anchoredPosition.x + (zonaDeAcerto.rect.width / 2f);

        if (posPonteiroX >= zonaInicioX && posPonteiroX <= zonaFimX)
        {
            Debug.Log("🎯 ACERTOU O SKILL CHECK!");
            TocarSFX(somAcerto);
            FinalizarSkillCheckSucesso();
        }
        else
        {
            Debug.Log("❌ ERROU O SKILL CHECK!");
            errosCometidos++;

            int limiteErros = modoAnaliseLaboratorio ? 1 : 3;

            if (errosCometidos >= limiteErros)
            {
                FinalizarComDerrota();
            }
            else
            {
                TocarSFX(somErro);
                ProximoSkillCheck();
            }
        }
    }

    void FinalizarSkillCheckSucesso()
    {
        if (painelSkillCheck != null) painelSkillCheck.SetActive(false);

        if (iconManager != null)
            iconManager.MudarParaNormal();

        RestaurarMusicaPrincipal();

        if (modoAnaliseLaboratorio && FishAnalyseManager.Instance != null)
        {
            FishAnalyseManager.Instance.OnSkillCheckSucesso();
        }
    }

    void FinalizarComDerrota()
    {
        jogoAtivo = false;
        if (painelSkillCheck != null) painelSkillCheck.SetActive(false);

        TocarSFX(somVomito);

        if (iconManager != null)
            iconManager.MudarParaNormal();

        RestaurarMusicaPrincipal();

        // Se estiver no laboratório de análise:
        if (modoAnaliseLaboratorio)
        {
            if (FishAnalyseManager.Instance != null)
            {
                FishAnalyseManager.Instance.OnSkillCheckFalha();
            }
            return;
        }

        // --- MODO PESCARIA ---
        // Desconta os peixes do contador simples do InventoryManager e GameController
        if (InventoryManager.Instance != null)
        {
            int perdidos = Mathf.Min(peixesPerdidosAoFalhar, InventoryManager.Instance.peixesColetados);
            InventoryManager.Instance.peixesColetados -= perdidos;

            if (gameController != null)
            {
                gameController.foundedFish -= perdidos;
                if (gameController.foundedFish < 0) gameController.foundedFish = 0;
            }

            if (respawnManager != null)
            {
                respawnManager.SpawnarPeixesEscondidos(perdidos);
            }
        }
    }

    void RestaurarMusicaPrincipal()
    {
        if (audioSourceSFX != null)
        {
            audioSourceSFX.Stop();
            audioSourceSFX.loop = false;
        }

        if (musicaPrincipal != null)
        {
            musicaPrincipal.UnPause();
        }
    }

    void TocarSFX(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }
}
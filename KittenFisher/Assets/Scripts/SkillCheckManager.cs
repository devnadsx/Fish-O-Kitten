using UnityEngine;

public class SkillCheckManager : MonoBehaviour
{
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
    public AudioClip somVomito; // 🤮 Arraste o áudio de vômito aqui!

    [Header("Gerenciadores")]
    public IconManager iconManager;
    public GameController gameController;
    public FishRespawnManager respawnManager;

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
        painelSkillCheck.SetActive(true);
        jogoAtivo = true;

        ponteiro.anchoredPosition = new Vector2(limiteEsquerda, ponteiro.anchoredPosition.y);
        movendoParaDireita = true;

        float metadeZona = zonaDeAcerto.rect.width / 2f;
        float xAleatorio = Random.Range(limiteEsquerda + metadeZona, limiteDireita - metadeZona);
        zonaDeAcerto.anchoredPosition = new Vector2(xAleatorio, zonaDeAcerto.anchoredPosition.y);
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

            if (errosCometidos >= 3)
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
        painelSkillCheck.SetActive(false);

        if (iconManager != null)
            iconManager.MudarParaNormal();

        RestaurarMusicaPrincipal();
    }

    void FinalizarComDerrota()
    {
        jogoAtivo = false;
        painelSkillCheck.SetActive(false);
        Debug.LogError("Você falhou 3 vezes no Skill Check!");

        // 🤮 Toca o som de vômito ao perder!
        TocarSFX(somVomito);

        if (iconManager != null)
            iconManager.MudarParaNormal();

        // Limpa a quantidade de peixes do inventário
        InventoryManager.Instance.LimparSlotsPorPunicao(peixesPerdidosAoFalhar);

        int perdidos = Mathf.Min(peixesPerdidosAoFalhar, InventoryManager.Instance.peixesNoInventario);
        InventoryManager.Instance.peixesNoInventario -= perdidos;

        if (gameController != null)
        {
            gameController.foundedFish -= perdidos;
            if (gameController.foundedFish < 0) gameController.foundedFish = 0;
        }

        if (respawnManager != null)
        {
            respawnManager.SpawnarPeixesEscondidos(perdidos);
        }

        RestaurarMusicaPrincipal();
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
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }
}
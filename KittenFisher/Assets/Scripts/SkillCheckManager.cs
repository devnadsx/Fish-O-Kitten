using UnityEngine;

public class SkillCheckManager : MonoBehaviour
{
    public static SkillCheckManager Instance;

    [Header("UI Elementos")]
    public GameObject painelSkillCheck;
    public RectTransform barraFundo;
    public RectTransform zonaDeAcerto;
    public RectTransform ponteiro;

    [Header("Configuração de Movimento")]
    public float velocidade = 600f;

    [Header("Sons")]
    public AudioSource musicaPrincipal;
    public AudioSource audioSourceSFX;
    public AudioClip musicaTensa;
    public AudioClip somAcerto;
    public AudioClip somErro;

    private bool movendoParaDireita = true;
    private bool jogoAtivo = false;

    private float limiteEsquerda;
    private float limiteDireita;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        AtualizarLimites();
    }

    void Update()
    {
        if (!jogoAtivo) return;

        // Movimentação da agulha/ponteiro
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

        // Pressionar Espaço para validar a tentativa
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ValidarClique();
        }
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

    public void IniciarSequenciaSkillCheck()
    {
        // Troca a música normal pela música tensa
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

        // Prepara e ativa a interface
        AtualizarLimites();
        if (painelSkillCheck != null) painelSkillCheck.SetActive(true);

        // Posiciona o ponteiro no início
        if (ponteiro != null)
            ponteiro.anchoredPosition = new Vector2(limiteEsquerda, ponteiro.anchoredPosition.y);

        movendoParaDireita = true;

        // Sortia a posição da Zona Amarela/Verde de acerto
        if (zonaDeAcerto != null)
        {
            float metadeZona = zonaDeAcerto.rect.width / 2f;
            float xAleatorio = Random.Range(limiteEsquerda + metadeZona, limiteDireita - metadeZona);
            zonaDeAcerto.anchoredPosition = new Vector2(xAleatorio, zonaDeAcerto.anchoredPosition.y);
        }

        jogoAtivo = true;
    }

    void ValidarClique()
    {
        jogoAtivo = false;

        float posPonteiroX = ponteiro.anchoredPosition.x;
        float zonaInicioX = zonaDeAcerto.anchoredPosition.x - (zonaDeAcerto.rect.width / 2f);
        float zonaFimX = zonaDeAcerto.anchoredPosition.x + (zonaDeAcerto.rect.width / 2f);

        // Checa se acertou dentro da zona
        if (posPonteiroX >= zonaInicioX && posPonteiroX <= zonaFimX)
        {
            TocarSFX(somAcerto);
            Finalizar(true);
        }
        else
        {
            TocarSFX(somErro);
            Finalizar(false);
        }
    }

    void Finalizar(bool sucesso)
    {
        if (painelSkillCheck != null) painelSkillCheck.SetActive(false);

        RestaurarMusicaPrincipal();

        // Notifica o FishAnalyseManager do resultado
        if (FishAnalyseManager.Instance != null)
        {
            if (sucesso)
                FishAnalyseManager.Instance.OnSkillCheckSucesso();
            else
                FishAnalyseManager.Instance.OnSkillCheckFalha();
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
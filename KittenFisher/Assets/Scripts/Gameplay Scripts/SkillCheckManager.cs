using System.Collections;
using UnityEngine;

public class SkillCheckManager : MonoBehaviour
{
    public static SkillCheckManager Instance;

    [Header("UI Elementos")]
    public GameObject painelSkillCheck;
    public RectTransform barraFundo;
    public RectTransform zonaDeAcerto;
    public RectTransform ponteiro;

    [Header("Referência da UI")]
    public MonoBehaviour uiManager;

    [Header("Configuração de Movimento")]
    public float velocidade = 600f;
    private float velocidadeBase; // 🟢 Guarda o valor padrão de velocidade

    [Header("Sons")]
    public AudioSource musicaPrincipal;
    public AudioSource audioSourceSFX;
    public AudioClip musicaTensa;
    public AudioClip somAcerto;
    public AudioClip somErro;

    private bool movendoParaDireita = true;
    private bool jogoAtivo = false;
    public bool estaAtivo => jogoAtivo;

    private float limiteEsquerda;
    private float limiteDireita;

    private string nomePeixeAtual = "";

    void Awake()
    {
        if (Instance == null) Instance = this;

        // 🟢 Salva a velocidade configurada no Inspector logo no início
        velocidadeBase = velocidade;
    }

    void Start()
    {
        AtualizarLimites();
    }

    void Update()
    {
        if (!jogoAtivo) return;

        // Usa o valor de velocidade ajustado para o peixe atual
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

    void AtualizarLimites()
    {
        if (barraFundo != null)
        {
            float larguraBarra = barraFundo.rect.width;
            limiteEsquerda = -larguraBarra / 2f;
            limiteDireita = larguraBarra / 2f;
        }
    }

    public void IniciarSequenciaSkillCheck(string nomePeixe, float multiplicadorVelocidade = 1.0f)
    {
        nomePeixeAtual = nomePeixe;

        // 🟢 AQUI ESTAVA O PROBLEMA: Agora aplicamos o multiplicador de velocidade
        velocidade = velocidadeBase * multiplicadorVelocidade;

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

        AtualizarLimites();
        if (painelSkillCheck != null) painelSkillCheck.SetActive(true);

        if (ponteiro != null)
            ponteiro.anchoredPosition = new Vector2(limiteEsquerda, ponteiro.anchoredPosition.y);

        movendoParaDireita = true;

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

        // 🟢 Reseta a velocidade para o padrão ao fechar
        velocidade = velocidadeBase;

        RestaurarMusicaPrincipal();

        if (GerenciadorAnalisePeixes.Instance != null)
        {
            if (sucesso)
                GerenciadorAnalisePeixes.Instance.OnSkillCheckSucesso();
            else
                GerenciadorAnalisePeixes.Instance.OnSkillCheckFalha();
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
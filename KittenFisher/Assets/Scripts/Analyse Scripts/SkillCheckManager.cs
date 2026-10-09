using System.Collections;
using UnityEngine;
using System;

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
    private float velocidadeBase;

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

    // 🟢 Novas variáveis para controle de sequências progressivas
    private Action<bool> callbackFinal;
    private int tentativasTotais = 1;
    private int tentativaAtual = 0;
    private float incrementoVelocidade = 1.25f;
    private float multiplicadorAtual = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        velocidadeBase = velocidade;
    }

    void Start()
    {
        AtualizarLimites();
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

    void AtualizarLimites()
    {
        if (barraFundo != null)
        {
            float larguraBarra = barraFundo.rect.width;
            limiteEsquerda = -larguraBarra / 2f;
            limiteDireita = larguraBarra / 2f;
        }
    }

    // Método tradicional mantido para os peixes
    public void IniciarSequenciaSkillCheck(string nomePeixe, float multiplicadorVelocidade = 1.0f)
    {
        nomePeixeAtual = nomePeixe;
        callbackFinal = null;
        tentativasTotais = 1;
        tentativaAtual = 0;
        multiplicadorAtual = multiplicadorVelocidade;

        ExecutarRodada();
    }

    // 🟢 NOVO MÉTODO: Suporta múltiplas rodadas com velocidade progressiva
    public void IniciarSequenciaMultipla(int totalRodadas, float multiplicadorInicial, float fatorAumento, Action<bool> onComplete)
    {
        nomePeixeAtual = "";
        tentativasTotais = totalRodadas;
        tentativaAtual = 0;
        multiplicadorAtual = multiplicadorInicial;
        incrementoVelocidade = fatorAumento;
        callbackFinal = onComplete;

        ExecutarRodada();
    }

    private void ExecutarRodada()
    {
        velocidade = velocidadeBase * multiplicadorAtual;

        if (musicaPrincipal != null && musicaPrincipal.isPlaying)
        {
            musicaPrincipal.Pause();
        }

        if (audioSourceSFX != null && musicaTensa != null && !audioSourceSFX.isPlaying)
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
            float xAleatorio = UnityEngine.Random.Range(limiteEsquerda + metadeZona, limiteDireita - metadeZona);
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
            tentativaAtual++;

            // Se ainda restam rodadas no teste
            if (tentativaAtual < tentativasTotais)
            {
                multiplicadorAtual *= incrementoVelocidade; // 🚀 Aumenta a velocidade
                ExecutarRodada();
            }
            else
            {
                Finalizar(true);
            }
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

        velocidade = velocidadeBase;
        RestaurarMusicaPrincipal();

        // Se veio do sistema de análise de peixes
        if (GerenciadorAnalisePeixes.Instance != null && string.IsNullOrEmpty(nomePeixeAtual) == false)
        {
            if (sucesso)
                GerenciadorAnalisePeixes.Instance.OnSkillCheckSucesso();
            else
                GerenciadorAnalisePeixes.Instance.OnSkillCheckFalha();
        }

        // 🟢 Se veio de uma chamada personalizada (ex: Tutorial / Montagem do Traje)
        callbackFinal?.Invoke(sucesso);
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
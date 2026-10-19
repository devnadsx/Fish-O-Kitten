
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class FalaItem
{
    [TextArea(2, 4)]
    public string texto;
    public bool ehNarracao;
}

public class AnalyseUIManager : MonoBehaviour
{
    [Header("UI Elementos")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Image imagemGatoUI;
    public RectTransform rectGatoUI;

    [Header("Expressões do Gato")]
    public Sprite bocaFechada;
    public Sprite bocaAberta;

    [Header("Cores de Fala vs. Narração")]
    public Color corGatoFala = Color.white;
    public Color corGatoNarracao = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Configurações do Pulo e Fala")]
    public float forcaPuloUI = 10f;
    public float velocidadeEscrita = 0.03f;
    public AudioSource audioSource;
    public AudioClip somFala;

    [Header("Próximas Cenas")]
    public string nomeCenaEntrevista = "End";
    public string nomeCenaFase3Normal = "Fase3";
    public string nomeCenaFase3Paxton = "Fase3 Paxton";

    private Vector2 posOriginalGato;
    private Coroutine coroutineFala;
    private Coroutine coroutinePulo;
    private bool estaEscrevendo = false;
    private string textoAtual = "";
    private float tempoUltimoInput = 0f;

    private List<FalaItem> sequenciaAtual = new List<FalaItem>();
    private int indiceFalaAtual = 0;

    private System.Action aoTerminarSequencia;

    void Awake()
    {
        if (rectGatoUI != null)
            posOriginalGato = rectGatoUI.anchoredPosition;
    }

    void Start()
    {
        List<FalaItem> intro = new List<FalaItem>()
        {
            new FalaItem
            {
                texto = "Hora de analisar esses peixes..",
                ehNarracao = false
            },
            new FalaItem
            {
                texto = "*Myke abre seu caderno e saca algumas ferramentas de pesquisa*",
                ehNarracao = true
            },
            new FalaItem
            {
                texto = "Ok.., por qual deles devo começar?",
                ehNarracao = false
            }
        };

        IniciarSequenciaDialogo(intro);
    }

    void Update()
    {
        if (Time.time - tempoUltimoInput < 0.2f)
            return;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            if (painelBalaoFala != null &&
                painelBalaoFala.activeSelf)
            {
                AvancarOuPularTexto();
            }
        }
    }

    public void IniciarSequenciaDialogo(
        List<FalaItem> falas,
        System.Action aoTerminar = null)
    {
        if (GerenciadorAnalisePeixes.Instance != null)
            GerenciadorAnalisePeixes.Instance.estaEmDialogo = true;

        sequenciaAtual = falas;
        indiceFalaAtual = 0;
        aoTerminarSequencia = aoTerminar;

        if (sequenciaAtual != null && sequenciaAtual.Count > 0)
        {
            MostrarFalaItem(sequenciaAtual[indiceFalaAtual]);
        }
        else
        {
            ConcluirSequenciaDialogo();
        }
    }

    public void AvancarOuPularTexto()
    {
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        indiceFalaAtual++;

        if (sequenciaAtual != null &&
            indiceFalaAtual < sequenciaAtual.Count)
        {
            MostrarFalaItem(sequenciaAtual[indiceFalaAtual]);
        }
        else
        {
            ConcluirSequenciaDialogo();
        }
    }

    private void ConcluirSequenciaDialogo()
    {
        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        estaEscrevendo = false;

        if (GerenciadorAnalisePeixes.Instance != null)
            GerenciadorAnalisePeixes.Instance.FinalizarDialogo();

        System.Action acao = aoTerminarSequencia;
        aoTerminarSequencia = null;

        if (acao != null)
            acao.Invoke();
    }

    private void MostrarFalaItem(FalaItem item)
    {
        tempoUltimoInput = Time.time;

        if (rectGatoUI != null)
            posOriginalGato = rectGatoUI.anchoredPosition;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (imagemGatoUI != null)
            imagemGatoUI.color =
                item.ehNarracao ? corGatoNarracao : corGatoFala;

        if (coroutineFala != null)
            StopCoroutine(coroutineFala);

        if (coroutinePulo != null)
            StopCoroutine(coroutinePulo);

        ResetaExpressao();

        coroutineFala = StartCoroutine(EfeitoDigitarItem(item));
    }

    IEnumerator EfeitoDigitarItem(FalaItem item)
    {
        estaEscrevendo = true;
        textoAtual = item.texto;

        if (textoBalao != null)
            textoBalao.text = "";

        yield return null;

        foreach (char letra in item.texto.ToCharArray())
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (!item.ehNarracao && char.IsLetterOrDigit(letra))
            {
                if (imagemGatoUI != null && bocaAberta != null)
                    imagemGatoUI.sprite = bocaAberta;

                if (audioSource != null && somFala != null)
                    audioSource.PlayOneShot(somFala);

                if (rectGatoUI != null)
                {
                    if (coroutinePulo != null)
                        StopCoroutine(coroutinePulo);

                    coroutinePulo = StartCoroutine(ExecutarPulinho());
                }
            }
            else
            {
                ResetaExpressao();
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        ResetaExpressao();
        estaEscrevendo = false;
        coroutineFala = null;
    }

    IEnumerator ExecutarPulinho()
    {
        if (rectGatoUI == null)
            yield break;

        rectGatoUI.anchoredPosition =
            posOriginalGato + new Vector2(0, forcaPuloUI);

        yield return new WaitForSeconds(0.04f);

        rectGatoUI.anchoredPosition = posOriginalGato;
        coroutinePulo = null;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineFala != null)
        {
            StopCoroutine(coroutineFala);
            coroutineFala = null;
        }

        if (textoBalao != null)
            textoBalao.text = textoAtual;

        ResetaExpressao();
        estaEscrevendo = false;
    }

    void ResetaExpressao()
    {
        if (rectGatoUI != null)
            rectGatoUI.anchoredPosition = posOriginalGato;

        if (imagemGatoUI != null && bocaFechada != null)
            imagemGatoUI.sprite = bocaFechada;
    }


    public void FinalizarAnalyseETrocarCena()
    {
        string cenaAtual = SceneManager.GetActiveScene().name;

        if (cenaAtual == "Analyse 1")
        {
            if (!string.IsNullOrWhiteSpace(nomeCenaEntrevista))
                SceneManager.LoadScene(nomeCenaEntrevista);

            return;
        }

        if (cenaAtual == "Analyse 2")
        {
            if (GameManager.Instance == null)
                return;

            bool paxtonVaiAcompanhar =
                GameManager.Instance.paxtonAcompanha;

            string proximaCena = paxtonVaiAcompanhar
                ? nomeCenaFase3Paxton
                : nomeCenaFase3Normal;

            if (!string.IsNullOrWhiteSpace(proximaCena))
                SceneManager.LoadScene(proximaCena);
        }
    }

}
    
    

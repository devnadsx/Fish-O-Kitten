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
    public bool ehNarracao; // Se true, o gato escurece, não mexe a boca e não pula (representa uma ação)
}

public class AnalyseUIManager : MonoBehaviour
{
    [Header("UI Elementos")]
    public GameObject painelBalaoFala;  // Objeto 'fala'
    public TextMeshProUGUI textoBalao; // Texto do balão
    public Image imagemGatoUI;         // GatinhoVN
    public RectTransform rectGatoUI;   // GatinhoVN

    [Header("Expressões do Gato")]
    public Sprite bocaFechada;
    public Sprite bocaAberta;

    [Header("Cores de Fala vs. Narração")]
    public Color corGatoFala = Color.white;
    public Color corGatoNarracao = new Color(0.6f, 0.6f, 0.6f, 1f); // Tom mais escuro durante ações

    [Header("Configurações do Pulo e Fala")]
    public float forcaPuloUI = 10f;
    public float velocidadeEscrita = 0.03f;
    public AudioSource audioSource;
    public AudioClip somFala;

    [Header("Próxima Cena")]
    public string nomeCenaEntrevista = "End";

    private Vector2 posOriginalGato;
    private Coroutine coroutineFala;
    private Coroutine coroutinePulo;
    private bool estaEscrevendo = false;
    private string textoAtual = "";
    private float tempoUltimoInput = 0f;

    // Fila para controlar a sequência de falas/narrações
    private List<FalaItem> sequenciaAtual = new List<FalaItem>();
    private int indiceFalaAtual = 0;

    void Awake()
    {
        if (rectGatoUI != null)
        {
            posOriginalGato = rectGatoUI.anchoredPosition;
        }
    }

    void Start()
    {
        // Introdução inicial com Fala + Narração de ação
        List<FalaItem> intro = new List<FalaItem>()
        {
            new FalaItem { texto = "Time to analyze these fish carefully...", ehNarracao = false },
            new FalaItem { texto = "*Mike opens his notebook and gets his tools ready.*", ehNarracao = true },
            new FalaItem { texto = "Which one should I start with?", ehNarracao = false }
        };

        IniciarSequenciaDialogo(intro);
    }

    void Update()
    {
        if (Time.time - tempoUltimoInput < 0.2f) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (painelBalaoFala != null && painelBalaoFala.activeSelf)
            {
                AvancarOuPularTexto();
            }
        }
    }

    public void IniciarSequenciaDialogo(List<FalaItem> falas)
    {
        sequenciaAtual = falas;
        indiceFalaAtual = 0;

        if (sequenciaAtual != null && sequenciaAtual.Count > 0)
        {
            MostrarFalaItem(sequenciaAtual[indiceFalaAtual]);
        }
    }

    public void AvancarOuPularTexto()
    {
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        // Avança para a próxima fala da sequência
        indiceFalaAtual++;

        if (sequenciaAtual != null && indiceFalaAtual < sequenciaAtual.Count)
        {
            MostrarFalaItem(sequenciaAtual[indiceFalaAtual]);
        }
        else
        {
            // Fim da sequência de falas atual
            if (painelBalaoFala != null)
                painelBalaoFala.SetActive(false);
        }
    }

    private void MostrarFalaItem(FalaItem item)
    {
        tempoUltimoInput = Time.time;

        if (rectGatoUI != null && (posOriginalGato == Vector2.zero || posOriginalGato.y == 0))
        {
            posOriginalGato = rectGatoUI.anchoredPosition;
        }

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        // Aplica o efeito visual de cor no Gato (fala vs narração)
        if (imagemGatoUI != null)
        {
            imagemGatoUI.color = item.ehNarracao ? corGatoNarracao : corGatoFala;
        }

        if (coroutineFala != null) StopCoroutine(coroutineFala);
        if (coroutinePulo != null) StopCoroutine(coroutinePulo);

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

            // Animação e áudio acontecem apenas se NÃO for narração
            if (!item.ehNarracao && char.IsLetterOrDigit(letra))
            {
                if (imagemGatoUI != null && bocaAberta != null)
                    imagemGatoUI.sprite = bocaAberta;

                if (audioSource != null && somFala != null)
                    audioSource.PlayOneShot(somFala);

                if (rectGatoUI != null)
                {
                    if (coroutinePulo != null) StopCoroutine(coroutinePulo);
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
    }

    IEnumerator ExecutarPulinho()
    {
        if (rectGatoUI == null) yield break;

        rectGatoUI.anchoredPosition = posOriginalGato + new Vector2(0, forcaPuloUI);
        yield return new WaitForSeconds(0.04f);
        rectGatoUI.anchoredPosition = posOriginalGato;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineFala != null)
            StopCoroutine(coroutineFala);

        if (textoBalao != null)
            textoBalao.text = textoAtual;

        ResetaExpressao();
        estaEscrevendo = false;
    }

    void ResetaExpressao()
    {
        if (rectGatoUI != null && posOriginalGato != Vector2.zero)
            rectGatoUI.anchoredPosition = posOriginalGato;

        if (imagemGatoUI != null && bocaFechada != null)
            imagemGatoUI.sprite = bocaFechada;
    }

    public void FinalizarAnalyseETrocarCena()
    {
        if (!string.IsNullOrEmpty(nomeCenaEntrevista))
        {
            SceneManager.LoadScene(nomeCenaEntrevista);
        }
    }
}
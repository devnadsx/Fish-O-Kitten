using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct LineDialogo
{
    [TextArea(2, 4)]
    public string texto;
    public bool ehAcaoOuNarracao;
}

public class DialogoManager : MonoBehaviour
{
    public static DialogoManager Instance;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Image imagemGatinho;
    public Button botaoAvancarFala;

    [Header("Efeitos Estilo Visual Novel (DDLC)")]
    public Color corFoco = Color.white;
    public Color corInativo = new Color(0.55f, 0.55f, 0.55f);
    public Vector3 escalaFoco = new Vector3(1f, 1f, 1f);
    public Vector3 escalaInativo = new Vector3(0.9f, 0.9f, 1f);
    public float velocidadeTransicao = 8f;

    [Header("Estilo Efeitos de Escrita & Som")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 8f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("Falas Configuráveis pelo Inspector")]
    public List<LineDialogo> falasIniciais = new List<LineDialogo>();

    private Queue<string> filaTextos = new Queue<string>();
    private Queue<bool> filaTiposAcao = new Queue<bool>();

    private Vector3 posicaoOriginalGato;
    private Coroutine coroutineDigitacao;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";
    private bool falaAtualEhAcao = false;

    private Vector3 escalaAlvo;
    private Color corAlvo;

    // Trava para evitar pulo duplo no Enter
    private float tempoUltimoClique = 0f;
    private float intervaloMinimoClique = 0.15f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (imagemGatinho != null)
        {
            posicaoOriginalGato = imagemGatinho.rectTransform.anchoredPosition;
            escalaAlvo = escalaFoco;
            corAlvo = corFoco;
        }

        if (falasIniciais.Count > 0)
        {
            IniciarSequenciaDialogo(falasIniciais);
        }
    }

    void Update()
    {
        if (imagemGatinho != null)
        {
            imagemGatinho.transform.localScale = Vector3.Lerp(imagemGatinho.transform.localScale, escalaAlvo, Time.deltaTime * velocidadeTransicao);
            imagemGatinho.color = Color.Lerp(imagemGatinho.color, corAlvo, Time.deltaTime * velocidadeTransicao);
        }

        if (painelBalaoFala != null && painelBalaoFala.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            AvancarTexto();
        }
    }

    public void IniciarSequenciaDialogo(List<LineDialogo> listaFalas)
    {
        LimparDialogo();

        foreach (var fala in listaFalas)
        {
            filaTextos.Enqueue(fala.texto);
            filaTiposAcao.Enqueue(fala.ehAcaoOuNarracao);
        }

        ExibirProximaFrase();
    }

    public void AdicionarFala(string texto, bool ehAcao = false)
    {
        filaTextos.Enqueue(texto);
        filaTiposAcao.Enqueue(ehAcao);

        if (painelBalaoFala != null && !painelBalaoFala.activeSelf && filaTextos.Count == 1)
        {
            ExibirProximaFrase();
        }
    }

    public void AvancarTexto()
    {
        // Trava anti-duplo clique
        if (Time.time - tempoUltimoClique < intervaloMinimoClique) return;
        tempoUltimoClique = Time.time;

        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        if (filaTextos.Count > 0)
        {
            ExibirProximaFrase();
        }
        else
        {
            FecharDialogo();
        }
    }

    public void ExibirProximaFrase()
    {
        if (filaTextos.Count == 0) return;

        string texto = filaTextos.Dequeue();
        bool ehAcao = filaTiposAcao.Dequeue();

        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(true);

        textoCompletoAtual = texto;
        falaAtualEhAcao = ehAcao;

        if (ehAcao)
        {
            corAlvo = corInativo;
            escalaAlvo = escalaInativo;
        }
        else
        {
            corAlvo = corFoco;
            escalaAlvo = escalaFoco;
        }

        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        coroutineDigitacao = StartCoroutine(EfeitoDigitar(texto));
    }

    IEnumerator EfeitoDigitar(string texto)
    {
        estaEscrevendo = true;
        textoBalao.text = "";

        foreach (char letra in texto.ToCharArray())
        {
            textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                if (!falaAtualEhAcao)
                {
                    if (imagemGatinho != null && imagemGatinho.gameObject.activeSelf)
                    {
                        StartCoroutine(PulinhoRapidoGato());
                    }

                    if (audioSourceSFX != null && somFalaGatinho != null)
                    {
                        audioSourceSFX.PlayOneShot(somFalaGatinho);
                    }
                }
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        textoBalao.text = textoCompletoAtual;
        estaEscrevendo = false;

        if (imagemGatinho != null)
        {
            imagemGatinho.rectTransform.anchoredPosition = posicaoOriginalGato;
        }
    }

    IEnumerator PulinhoRapidoGato()
    {
        RectTransform rect = imagemGatinho.rectTransform;
        rect.anchoredPosition = posicaoOriginalGato + new Vector3(0, alturaPuloLetra, 0);
        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);
        rect.anchoredPosition = posicaoOriginalGato;
    }

    public bool TemFalasPendentes()
    {
        return filaTextos.Count > 0 || estaEscrevendo;
    }

    public void LimparDialogo()
    {
        filaTextos.Clear();
        filaTiposAcao.Clear();
    }

    public void FecharDialogo()
    {
        LimparDialogo();
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
    }
}
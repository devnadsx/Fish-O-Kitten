
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneIntroDialogue : MonoBehaviour
{
    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Sprites do Gatinho (Boca Aberta / Fechada)")]
    public Image imagemGatinho;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;

    [Header("Estilo Visual Novel / RPG")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloPalavra = 6f;
    public float duracaoPuloPalavra = 0.12f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("Escolha do diálogo")]
    public bool paxtonAtivado;

    [Header("Falas - Myke sozinho")]
    [TextArea(2, 5)]
    public List<string> falasMykeSozinho = new List<string>();

    [Header("Falas - Paxton ativado (com Buliana)")]
    [TextArea(2, 5)]
    public List<string> falasComPaxton = new List<string>();

    [Header("Scripts bloqueados durante o diálogo")]
    [Tooltip("Arraste os COMPONENTES dos scripts aqui, não os GameObjects.")]
    public Behaviour[] scriptsBloquearDuranteDialogo;

    private readonly List<Behaviour> scriptsQueEstavamAtivos = new List<Behaviour>();
    private readonly List<bool> estadosOriginaisScripts = new List<bool>();

    private Vector2 posicaoOriginalGato;
    private Coroutine coroutineEscrita;
    private Coroutine coroutinePulo;
    private bool estaEscrevendo;
    private bool dialogoAtivo;
    private string textoCompletoAtual = "";
    private int indiceFalaAtual;
    private List<string> falasAtuais;

    private void Awake()
    {
        if (imagemGatinho != null)
        {
            posicaoOriginalGato = imagemGatinho.rectTransform.anchoredPosition;
            DefinirSpriteNormal();
        }
    }

    private void Start()
    {
        paxtonAtivado = GameManager.Instance != null &&
                        GameManager.Instance.paxtonAcompanha;

        IniciarDialogo(paxtonAtivado);
    }

    private void Update()
    {
        if (!dialogoAtivo)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            AvancarTexto();
    }
    public void IniciarDialogoComFalas(List<string> novasFalas)
    {
        if (novasFalas == null || novasFalas.Count == 0)
            return;

        PararDigitacaoEPulo();

        falasAtuais = new List<string>(novasFalas);
        indiceFalaAtual = 0;

        DefinirEstadoGameplay(false);

        dialogoAtivo = true;

        painelBalaoFala.SetActive(true);
        imagemGatinho.gameObject.SetActive(true);
        botaoAvancarFala.gameObject.SetActive(true);

        MostrarFalaAtual();
    }

    private void IniciarDialogo(bool comPaxton)
    {
        PararDigitacaoEPulo();

        paxtonAtivado = comPaxton;
        falasAtuais = comPaxton ? falasComPaxton : falasMykeSozinho;

        DefinirEstadoGameplay(false);

        dialogoAtivo = true;
        indiceFalaAtual = 0;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (imagemGatinho != null)
            imagemGatinho.gameObject.SetActive(true);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(true);

        if (textoBalao != null)
            textoBalao.text = "";

        if (falasAtuais != null && falasAtuais.Count > 0)
            MostrarFalaAtual();
        else
            FinalizarDialogo();
    }

    public void IniciarNovoDialogoExterno()
    {
        IniciarDialogo(paxtonAtivado);
    }

    public void IniciarNovoDialogoExterno(bool comPaxton)
    {
        IniciarDialogo(comPaxton);
    }

    public void AvancarTexto()
    {
        if (!dialogoAtivo)
            return;

        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        indiceFalaAtual++;

        if (falasAtuais != null && indiceFalaAtual < falasAtuais.Count)
            MostrarFalaAtual();
        else
            FinalizarDialogo();
    }

    private void MostrarFalaAtual()
    {
        if (falasAtuais == null || indiceFalaAtual < 0 ||
            indiceFalaAtual >= falasAtuais.Count)
        {
            FinalizarDialogo();
            return;
        }

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (imagemGatinho != null)
            imagemGatinho.gameObject.SetActive(true);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(true);

        IniciarDigitacao(falasAtuais[indiceFalaAtual]);
    }

    private void IniciarDigitacao(string texto)
    {
        textoCompletoAtual = texto ?? "";

        if (coroutineEscrita != null)
            StopCoroutine(coroutineEscrita);

        coroutineEscrita = StartCoroutine(EfeitoDigitarTexto(textoCompletoAtual));
    }

    private IEnumerator EfeitoDigitarTexto(string texto)
    {
        estaEscrevendo = true;

        if (textoBalao != null)
            textoBalao.text = "";

        bool dentroDeUmaPalavra = false;

        foreach (char letra in texto)
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            bool caractereDePalavra = char.IsLetterOrDigit(letra);

            if (caractereDePalavra)
            {
                if (!dentroDeUmaPalavra)
                    IniciarPulinhoPalavra();

                dentroDeUmaPalavra = true;

                if (imagemGatinho != null && spriteBocaAberta != null)
                    imagemGatinho.sprite = spriteBocaAberta;

                if (audioSourceSFX != null && somFalaGatinho != null)
                    audioSourceSFX.PlayOneShot(somFalaGatinho);
            }
            else
            {
                dentroDeUmaPalavra = false;
                DefinirSpriteNormal();
            }

            yield return new WaitForSeconds(Mathf.Max(0.001f, velocidadeEscrita));
        }

        DefinirSpriteNormal();
        estaEscrevendo = false;
        coroutineEscrita = null;
    }

    private void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null)
        {
            StopCoroutine(coroutineEscrita);
            coroutineEscrita = null;
        }

        if (textoBalao != null)
            textoBalao.text = textoCompletoAtual;

        DefinirSpriteNormal();
        estaEscrevendo = false;

        PararPulinho();
    }

    private void IniciarPulinhoPalavra()
    {
        if (imagemGatinho == null || !imagemGatinho.gameObject.activeInHierarchy)
            return;

        if (coroutinePulo != null)
            StopCoroutine(coroutinePulo);

        coroutinePulo = StartCoroutine(PulinhoRapidoGato());
    }

    private IEnumerator PulinhoRapidoGato()
    {
        RectTransform rect = imagemGatinho.rectTransform;
        float duracao = Mathf.Max(0.01f, duracaoPuloPalavra);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);
            float altura = Mathf.Sin(progresso * Mathf.PI) * alturaPuloPalavra;

            rect.anchoredPosition = posicaoOriginalGato + Vector2.up * altura;

            yield return null;
        }

        rect.anchoredPosition = posicaoOriginalGato;
        coroutinePulo = null;
    }

    private void PararPulinho()
    {
        if (coroutinePulo != null)
        {
            StopCoroutine(coroutinePulo);
            coroutinePulo = null;
        }

        if (imagemGatinho != null)
            imagemGatinho.rectTransform.anchoredPosition = posicaoOriginalGato;
    }

    private void PararDigitacaoEPulo()
    {
        if (coroutineEscrita != null)
        {
            StopCoroutine(coroutineEscrita);
            coroutineEscrita = null;
        }

        estaEscrevendo = false;
        PararPulinho();
    }

    private void DefinirSpriteNormal()
    {
        if (imagemGatinho != null && spriteBocaFechada != null)
            imagemGatinho.sprite = spriteBocaFechada;
    }

    private void FinalizarDialogo()
    {
        PararDigitacaoEPulo();
        DefinirSpriteNormal();

        dialogoAtivo = false;
        estaEscrevendo = false;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        if (imagemGatinho != null)
            imagemGatinho.gameObject.SetActive(false);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(false);

        DefinirEstadoGameplay(true);
    }

    private void DefinirEstadoGameplay(bool estado)
    {
        if (!estado)
        {
            scriptsQueEstavamAtivos.Clear();
            estadosOriginaisScripts.Clear();

            if (scriptsBloquearDuranteDialogo == null)
                return;

            foreach (Behaviour script in scriptsBloquearDuranteDialogo)
            {
                if (script == null || scriptsQueEstavamAtivos.Contains(script))
                    continue;

                scriptsQueEstavamAtivos.Add(script);
                estadosOriginaisScripts.Add(script.enabled);

                script.enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < scriptsQueEstavamAtivos.Count; i++)
            {
                if (scriptsQueEstavamAtivos[i] != null)
                    scriptsQueEstavamAtivos[i].enabled = estadosOriginaisScripts[i];
            }

            scriptsQueEstavamAtivos.Clear();
            estadosOriginaisScripts.Clear();
        }
    }
}

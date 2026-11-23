
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneIntroDialogue : MonoBehaviour
{
    [System.Serializable]
    public class FalaIntro
    {
        [Header("Fala")]
        public string nomeQuemFala = "Myke";

        [TextArea(2, 5)]
        public string texto;

        public bool ehNarracao;

        public AudioClip somFala;
    }

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Imagem e expressões do Myke")]
    public Image imagemGatinho;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;

    [Header("Imagem e expressões do Paxton")]
    public Image imagemPaxton;
    public Sprite spritePaxtonBocaFechada;
    public Sprite spritePaxtonBocaAberta;

    [Header("Estilo Visual Novel / RPG")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloPalavra = 6f;
    public float duracaoPuloPalavra = 0.12f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("Escolha do diálogo")]
    public bool paxtonAtivado;

    [Header("Falas - Myke sozinho")]
    public List<FalaIntro> falasMykeSozinho = new List<FalaIntro>();

    [Header("Falas - Paxton ativado (com Buliana)")]
    public List<FalaIntro> falasComPaxton = new List<FalaIntro>();

    [Header("Scripts bloqueados durante o diálogo")]
    [Tooltip("Arraste os COMPONENTES dos scripts aqui, não os GameObjects.")]
    public Behaviour[] scriptsBloquearDuranteDialogo;

    [Header("Controle da Câmera")]
    public ControleCamera2D controleCamera;

    private readonly List<Behaviour> scriptsQueEstavamAtivos = new List<Behaviour>();
    private readonly List<bool> estadosOriginaisScripts = new List<bool>();

    private Vector2 posicaoOriginalGato;
    private Vector2 posicaoOriginalPaxton;

    private Coroutine coroutineEscrita;
    private Coroutine coroutinePulo;

    private bool estaEscrevendo;
    private bool dialogoAtivo;

    private string textoCompletoAtual = "";
    private int indiceFalaAtual;

    private List<FalaIntro> falasAtuais;

    private void Awake()
    {
        if (imagemGatinho != null)
            posicaoOriginalGato = imagemGatinho.rectTransform.anchoredPosition;

        if (imagemPaxton != null)
            posicaoOriginalPaxton = imagemPaxton.rectTransform.anchoredPosition;

        DefinirSpriteNormal("Myke");
        DefinirSpriteNormal("Paxton");
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

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            AvancarTexto();
        }
    }

    private void IniciarDialogo(bool comPaxton)
    {
        PararDigitacaoEPulo();

        paxtonAtivado = comPaxton;

        falasAtuais = comPaxton
            ? falasComPaxton
            : falasMykeSozinho;

        DefinirEstadoGameplay(false);

        dialogoAtivo = true;
        indiceFalaAtual = 0;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (imagemGatinho != null)
            imagemGatinho.gameObject.SetActive(true);

        if (imagemPaxton != null)
            imagemPaxton.gameObject.SetActive(comPaxton);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(true);

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

    // Compatibilidade com objetos que ainda enviam List<string>
    public void IniciarDialogoComFalas(List<string> novasFalas)
    {
        if (novasFalas == null || novasFalas.Count == 0)
            return;

        List<FalaIntro> falasConvertidas = new List<FalaIntro>();

        foreach (string texto in novasFalas)
        {
            falasConvertidas.Add(new FalaIntro
            {
                nomeQuemFala = "Myke",
                texto = texto
            });
        }

        IniciarDialogoComFalasDetalhadas(falasConvertidas);
    }

    public void IniciarDialogoComFalasDetalhadas(List<FalaIntro> novasFalas)
    {
        if (novasFalas == null || novasFalas.Count == 0)
            return;

        PararDigitacaoEPulo();

        falasAtuais = new List<FalaIntro>(novasFalas);
        indiceFalaAtual = 0;

        DefinirEstadoGameplay(false);

        dialogoAtivo = true;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (imagemGatinho != null)
            imagemGatinho.gameObject.SetActive(true);

        if (imagemPaxton != null)
            imagemPaxton.gameObject.SetActive(true);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(true);

        MostrarFalaAtual();
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
        if (falasAtuais == null ||
            indiceFalaAtual < 0 ||
            indiceFalaAtual >= falasAtuais.Count)
        {
            FinalizarDialogo();
            return;
        }

        FalaIntro fala = falasAtuais[indiceFalaAtual];

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text = fala.ehNarracao
                ? ""
                : fala.nomeQuemFala;
        }

        DefinirSpriteNormal("Myke");
        DefinirSpriteNormal("Paxton");

        IniciarDigitacao(fala);
    }

    private void IniciarDigitacao(FalaIntro fala)
    {
        textoCompletoAtual = fala.texto ?? "";

        if (coroutineEscrita != null)
            StopCoroutine(coroutineEscrita);

        coroutineEscrita = StartCoroutine(EfeitoDigitarTexto(fala));
    }

    private IEnumerator EfeitoDigitarTexto(FalaIntro fala)
    {
        estaEscrevendo = true;

        if (textoBalao != null)
            textoBalao.text = "";

        string personagem = fala.nomeQuemFala;
        Image imagemAtiva = ObterImagem(personagem);

        bool dentroDeUmaPalavra = false;

        foreach (char letra in textoCompletoAtual)
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (!fala.ehNarracao && char.IsLetterOrDigit(letra))
            {
                if (!dentroDeUmaPalavra)
                    IniciarPulinhoPalavra(imagemAtiva);

                dentroDeUmaPalavra = true;

                DefinirSpriteFalando(personagem);

                AudioClip som = fala.somFala != null
                    ? fala.somFala
                    : somFalaGatinho;

                if (audioSourceSFX != null && som != null)
                    audioSourceSFX.PlayOneShot(som);
            }
            else
            {
                dentroDeUmaPalavra = false;
                DefinirSpriteNormal(personagem);
            }

            yield return new WaitForSeconds(
                Mathf.Max(0.001f, velocidadeEscrita)
            );
        }

        DefinirSpriteNormal(personagem);

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

        if (falasAtuais != null &&
            indiceFalaAtual >= 0 &&
            indiceFalaAtual < falasAtuais.Count)
        {
            DefinirSpriteNormal(
                falasAtuais[indiceFalaAtual].nomeQuemFala
            );
        }

        estaEscrevendo = false;
        PararPulinho();
    }

    private Image ObterImagem(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return imagemGatinho;

        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return imagemPaxton;

        return null;
    }

    private void DefinirSpriteFalando(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        {
            if (imagemGatinho != null && spriteBocaAberta != null)
                imagemGatinho.sprite = spriteBocaAberta;
        }
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        {
            if (imagemPaxton != null && spritePaxtonBocaAberta != null)
                imagemPaxton.sprite = spritePaxtonBocaAberta;
        }
    }

    private void DefinirSpriteNormal(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        {
            if (imagemGatinho != null && spriteBocaFechada != null)
                imagemGatinho.sprite = spriteBocaFechada;
        }
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        {
            if (imagemPaxton != null && spritePaxtonBocaFechada != null)
                imagemPaxton.sprite = spritePaxtonBocaFechada;
        }
    }

    private void IniciarPulinhoPalavra(Image imagem)
    {
        if (imagem == null || !imagem.gameObject.activeInHierarchy)
            return;

        if (coroutinePulo != null)
            StopCoroutine(coroutinePulo);

        coroutinePulo = StartCoroutine(PulinhoRapidoGato(imagem));
    }

    private IEnumerator PulinhoRapidoGato(Image imagem)
    {
        RectTransform rect = imagem.rectTransform;

        Vector2 posicaoOriginal = imagem == imagemPaxton
            ? posicaoOriginalPaxton
            : posicaoOriginalGato;

        float duracao = Mathf.Max(0.01f, duracaoPuloPalavra);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);
            float altura = Mathf.Sin(progresso * Mathf.PI) * alturaPuloPalavra;

            rect.anchoredPosition = posicaoOriginal + Vector2.up * altura;

            yield return null;
        }

        rect.anchoredPosition = posicaoOriginal;
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

        if (imagemPaxton != null)
            imagemPaxton.rectTransform.anchoredPosition = posicaoOriginalPaxton;
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

    private void FinalizarDialogo()
    {
        PararDigitacaoEPulo();

        DefinirSpriteNormal("Myke");
        DefinirSpriteNormal("Paxton");

        dialogoAtivo = false;
        estaEscrevendo = false;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        if (imagemGatinho != null)
            imagemGatinho.gameObject.SetActive(false);

        if (imagemPaxton != null)
            imagemPaxton.gameObject.SetActive(false);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(false);

        DefinirEstadoGameplay(true);
    }

    private void DefinirEstadoGameplay(bool estado)
    {
        if (controleCamera != null)
            controleCamera.BloquearMovimento(!estado);

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

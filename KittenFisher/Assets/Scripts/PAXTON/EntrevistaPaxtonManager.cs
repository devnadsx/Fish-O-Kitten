using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EntrevistaPaxtonManager : MonoBehaviour
{
    [System.Serializable]

    public class ExpressaoPersonagem
    {
        public string nomeExpressao;
        public Sprite bocaFechada;
        public Sprite bocaAberta;
    }

    [System.Serializable]
    public class Personagem
    {
        public string nome;
        public List<ExpressaoPersonagem> expressoes =
            new List<ExpressaoPersonagem>();
    }

    [System.Serializable]
    public class FalaEntrevista
    {
        [Header("Fala")]
        public string nomeQuemFala;
        [TextArea(2, 5)]
        public string texto;
        public bool ehNarracao;
        public PosicaoPersonagemEntrevista posicao;
        public AudioClip somFala;

        [Header("Expressões")]
        public string expressaoMyke;
        public string expressaoPaxton;

        [Header("Movimento do Paxton")]
        public bool moverPaxton;
        public float deslocamentoX;
        public float deslocamentoY;
        public float duracaoMovimento = 0.5f;
    }

    public enum PosicaoPersonagemEntrevista
    {
        Esquerda,
        Centro,
        Direita
    }

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;

    [Header("Imagens dos Personagens")]
    public Image imagemMyke;
    public Image imagemPaxton;
    public Image imagemSaky;

    [Header("Personagens")]
    public Personagem myke;
    public Personagem paxton;
    public Personagem saky;

    [Header("Falas da Entrevista")]
    public List<FalaEntrevista> falas = new List<FalaEntrevista>();

    [Header("Configurações de Digitação")]
    public float velocidadeEscrita = 0.03f;
    public AudioSource audioSourceFala;
    public AudioClip somFalaPadrao;

    [Header("Configurações de Movimento")]
    public float velocidadeTransicaoExpressao = 0.25f;

    [Header("Final da Entrevista")]
    public string nomeProximaCena = "";

    private int indiceFala;
    private bool escrevendo;
    private bool entrevistaFinalizada;

    private string textoCompleto;
    private Coroutine coroutineDigitacao;
    private Coroutine coroutineMovimento;

    private Vector2 posicaoOriginalMyke;
    private Vector2 posicaoOriginalPaxton;

    private ExpressaoPersonagem expressaoAtualMyke;
    private ExpressaoPersonagem expressaoAtualPaxton;
    private ExpressaoPersonagem expressaoAtualSaky;

    private void Awake()
    {
        if (imagemMyke != null)
            posicaoOriginalMyke = imagemMyke.rectTransform.anchoredPosition;

        if (imagemPaxton != null)
            posicaoOriginalPaxton = imagemPaxton.rectTransform.anchoredPosition;
    }

    private void Start()
    {
        if (falas.Count > 0)
        {
            indiceFala = 0;
            entrevistaFinalizada = false;

            if (painelBalaoFala != null)
                painelBalaoFala.SetActive(true);

            MostrarFala();
        }
        else
        {
            FinalizarEntrevista();
        }
    }

    private void Update()
    {
        if (entrevistaFinalizada)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            AvancarFala();
        }
    }

    public void AvancarFala()
    {
        if (entrevistaFinalizada)
            return;

        if (escrevendo)
        {
            CompletarTexto();
            return;
        }

        indiceFala++;

        if (indiceFala < falas.Count)
        {
            MostrarFala();
        }
        else
        {
            FinalizarEntrevista();
        }
    }

    private void MostrarFala()
    {
        FalaEntrevista fala = falas[indiceFala];

        textoCompleto = fala.texto;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text =
                fala.ehNarracao ? "" : fala.nomeQuemFala;
        }

        AplicarExpressoes(fala);
        AplicarMovimento(fala);

        if (coroutineDigitacao != null)
            StopCoroutine(coroutineDigitacao);

        coroutineDigitacao = StartCoroutine(DigitarTexto(fala));
    }

    private IEnumerator DigitarTexto(FalaEntrevista fala)
    {
        escrevendo = true;

        if (textoBalao != null)
            textoBalao.text = "";

        Personagem personagem = ObterPersonagem(fala.nomeQuemFala);
        Image imagemAtiva = ObterImagem(fala.nomeQuemFala);

        foreach (char letra in fala.texto)
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (!fala.ehNarracao && char.IsLetterOrDigit(letra))
            {
                ExpressaoPersonagem expressao = ObterExpressaoAtual(fala.nomeQuemFala);

                if (imagemAtiva != null && expressao != null &&
                    expressao.bocaAberta != null)
                {
                    imagemAtiva.sprite = expressao.bocaAberta;
                }

                AudioClip som = fala.somFala != null
                    ? fala.somFala
                    : somFalaPadrao;

                if (audioSourceFala != null && som != null)
                    audioSourceFala.PlayOneShot(som);
            }
            else
            {
                RestaurarExpressaoDoFalante(fala.nomeQuemFala);
            }

            yield return new WaitForSeconds(velocidadeEscrita);

            RestaurarExpressaoDoFalante(fala.nomeQuemFala);
        }

        RestaurarExpressaoDoFalante(fala.nomeQuemFala);

        escrevendo = false;
    }

    private void CompletarTexto()
    {
        if (coroutineDigitacao != null)
            StopCoroutine(coroutineDigitacao);

        if (textoBalao != null)
            textoBalao.text = textoCompleto;

        RestaurarExpressaoDoFalante(falas[indiceFala].nomeQuemFala);

        escrevendo = false;
    }

    private void AplicarExpressoes(FalaEntrevista fala)
    {
        if (!string.IsNullOrWhiteSpace(fala.expressaoMyke))
        {
            ExpressaoPersonagem nova = EncontrarExpressao(myke, fala.expressaoMyke);
            if (nova != null)
                expressaoAtualMyke = nova;
            RestaurarExpressao("Myke");
        }

        if (!string.IsNullOrWhiteSpace(fala.expressaoPaxton))
        {
            ExpressaoPersonagem nova = EncontrarExpressao(paxton, fala.expressaoPaxton);
            if (nova != null)
                expressaoAtualPaxton = nova;
            RestaurarExpressao("Paxton");
        }

        if (expressaoAtualSaky == null && saky != null &&
            saky.expressoes != null && saky.expressoes.Count > 0)
            expressaoAtualSaky = saky.expressoes[0];

        RestaurarExpressaoDoFalante(fala.nomeQuemFala);
    }

    private ExpressaoPersonagem EncontrarExpressao(Personagem personagem, string nomeExpressao)
    {
        if (personagem == null || personagem.expressoes == null ||
            string.IsNullOrWhiteSpace(nomeExpressao))
            return null;

        foreach (ExpressaoPersonagem expressao in personagem.expressoes)
        {
            if (expressao != null &&
                nomeExpressao.Equals(expressao.nomeExpressao,
                    System.StringComparison.OrdinalIgnoreCase))
                return expressao;
        }

        Debug.LogWarning("Expressão '" + nomeExpressao +
            "' não encontrada para " + personagem.nome + ".");
        return null;
    }

    private ExpressaoPersonagem ObterExpressaoAtual(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return expressaoAtualMyke;

        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return expressaoAtualPaxton;

        if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            return expressaoAtualSaky;

        return null;
    }

    private void RestaurarExpressao(string nome)
    {
        Image imagem = ObterImagem(nome);
        ExpressaoPersonagem expressao = ObterExpressaoAtual(nome);

        if (imagem != null && expressao != null && expressao.bocaFechada != null)
            imagem.sprite = expressao.bocaFechada;
    }

    private void RestaurarExpressaoDoFalante(string nome)
    {
        RestaurarExpressao(nome);
    }

    private void AplicarMovimento(FalaEntrevista fala)
    {
        if (!fala.moverPaxton || imagemPaxton == null)
            return;

        if (coroutineMovimento != null)
            StopCoroutine(coroutineMovimento);

        Vector2 destino = posicaoOriginalPaxton +
            new Vector2(fala.deslocamentoX, fala.deslocamentoY);

        coroutineMovimento = StartCoroutine(
            MoverPaxton(destino, fala.duracaoMovimento)
        );
    }

    private IEnumerator MoverPaxton(Vector2 destino, float duracao)
    {
        RectTransform rect = imagemPaxton.rectTransform;
        Vector2 inicio = rect.anchoredPosition;

        if (duracao <= 0f)
        {
            rect.anchoredPosition = destino;
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);

            rect.anchoredPosition = Vector2.Lerp(
                inicio,
                destino,
                progresso
            );

            yield return null;
        }

        rect.anchoredPosition = destino;
    }

    private Personagem ObterPersonagem(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (myke != null &&
            nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return myke;

        if (paxton != null &&
            nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return paxton;

        if (saky != null &&
            nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            return saky;

        return null;
    }

    private Image ObterImagem(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return imagemMyke;

        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return imagemPaxton;

        if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            return imagemSaky;

        return null;
    }

    private void FinalizarEntrevista()
    {
        entrevistaFinalizada = true;
        escrevendo = false;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        if (!string.IsNullOrEmpty(nomeProximaCena))
            SceneManager.LoadScene(nomeProximaCena);
    }

    public void ReiniciarEntrevista()
    {
        if (coroutineDigitacao != null)
            StopCoroutine(coroutineDigitacao);

        if (coroutineMovimento != null)
            StopCoroutine(coroutineMovimento);

        if (imagemMyke != null)
        {
            imagemMyke.rectTransform.anchoredPosition =
                posicaoOriginalMyke;
        }

        if (imagemPaxton != null)
        {
            imagemPaxton.rectTransform.anchoredPosition =
                posicaoOriginalPaxton;
        }

        expressaoAtualMyke = myke != null && myke.expressoes != null &&
            myke.expressoes.Count > 0 ? myke.expressoes[0] : null;
        expressaoAtualPaxton = paxton != null && paxton.expressoes != null &&
            paxton.expressoes.Count > 0 ? paxton.expressoes[0] : null;
        expressaoAtualSaky = saky != null && saky.expressoes != null &&
            saky.expressoes.Count > 0 ? saky.expressoes[0] : null;

        RestaurarExpressao("Myke");
        RestaurarExpressao("Paxton");
        RestaurarExpressao("Saky");

        indiceFala = 0;
        entrevistaFinalizada = false;

        MostrarFala();
    }
}